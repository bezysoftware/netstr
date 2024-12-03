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
        private readonly IEnumerable<IEventValidator> validators;
        private readonly IHttpContextAccessor http;

        public AuthMessageHandler(
            ILogger<AuthMessageHandler> logger,
            IEnumerable<IEventValidator> validators,
            IHttpContextAccessor http)
        {
            this.logger = logger;
            this.validators = validators;
            this.http = http;
        }

        public bool CanHandleMessage(string type) => type == MessageType.Auth;

        public Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            var e = ValidateAuthEvent(parameters, adapter.Context);

            this.logger.LogInformation($"Authenticating client {adapter.Context.ClientId}.");

            adapter.Context.Authenticate(e.PublicKey);

            this.logger.LogInformation($"Client {adapter.Context.ClientId} successfully authenticated.");

            adapter.SendOk(e.Id);

            return Task.CompletedTask;
        }

        private Event ValidateAuthEvent(JsonDocument[] parameters, ClientContext context)
        {
            var ctx = this.http.HttpContext?.Request ?? throw new InvalidOperationException("HttpContext not set");
            var e = EventParser.TryParse(parameters, out var ex) ?? throw new UnknownMessageProcessingException(Messages.ErrorProcessingEvent);
            var validation = this.validators.ValidateEvent(e, context);

            if (validation != null)
            {
                throw new EventProcessingException(e, validation);
            }

            if (e.Kind != EventKind.Auth)
            {
                throw new EventProcessingException(e, Messages.AuthRequiredWrongKind);
            }

            var challenge = e.Tags.FirstOrDefault(x => x.Length == 2 && x[0] == EventTag.Challenge);
            if (challenge == null || challenge[1] != context.Challenge)
            {
                throw new EventProcessingException(e, Messages.AuthRequiredWrongTags);
            }

            var path = ctx.GetNormalizedUrl();
            var relays = e.GetNormalizedRelayValues();
            
            if (!relays.Any(x => x == path))
            {
                throw new EventProcessingException(e, Messages.AuthRequiredWrongTags);
            }

            return e;
        }
    }
}
