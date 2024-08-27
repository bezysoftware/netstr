using Microsoft.Extensions.Options;
using Netstr.Extensions;
using Netstr.Messaging.Events;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    public class AuthMessageHandler : IMessageHandler
    {
        private readonly ILogger<AuthMessageHandler> logger;
        private readonly IOptions<ConnectionOptions> connection;
        private readonly IEnumerable<IEventValidator> validators;
        private readonly IHttpContextAccessor http;

        public AuthMessageHandler(
            ILogger<AuthMessageHandler> logger,
            IOptions<ConnectionOptions> connection,
            IEnumerable<IEventValidator> validators,
            IHttpContextAccessor http)
        {
            this.logger = logger;
            this.connection = connection;
            this.validators = validators;
            this.http = http;
        }

        public bool CanHandleMessage(string type) => type == MessageType.Auth;

        public async Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            var e = ValidateAuthEvent(parameters, adapter.Context);

            adapter.Context.Authenticate(e.PublicKey);

            this.logger.LogInformation($"Client {adapter.Context.ClientId} successfully authenticated.");

            await adapter.SendOkAsync(e.Id);
        }

        private Event ValidateAuthEvent(JsonDocument[] parameters, ClientContext context)
        {
            var ctx = this.http.HttpContext?.Request ?? throw new InvalidOperationException("HttpContext not set");
            var e = EventParser.TryParse(parameters, out var ex);

            if (e == null)
            {
                throw new MessageProcessingException(Messages.ErrorProcessingEvent);
            }

            var validation = this.validators.ValidateEvent(e);

            if (validation != null)
            {
                throw new MessageProcessingException(e, validation);
            }

            if (e.Kind != EventKind.Auth)
            {
                throw new MessageProcessingException(e, Messages.AuthRequiredWrongKind);
            }

            var challenge = e.Tags.FirstOrDefault(x => x.Length == 2 && x[0] == EventTag.Challenge);
            if (challenge == null || challenge[1] != context.Challenge)
            {
                throw new MessageProcessingException(e, Messages.AuthRequiredWrongTags);
            }

            var path = $"{ctx.Host}{ctx.Path}".TrimEnd('/');
            var relayTag = e.Tags.FirstOrDefault(x => x.Length == 2 && x[0] == EventTag.Relay);
            var relay = relayTag?[1].Split("://")[1].TrimEnd('/');
            if (relayTag == null || relay != path)
            {
                throw new MessageProcessingException(e, Messages.AuthRequiredWrongTags);
            }

            return e;
        }
    }
}
