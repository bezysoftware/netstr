using Microsoft.Extensions.Options;
using Netstr.Messaging.Events;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes EVENT messages.
    /// </summary>
    public class EventMessageHandler : IMessageHandler
    {
        private readonly ILogger<EventMessageHandler> logger;
        private readonly IEventDispatcher eventDispatcher;
        private readonly IEnumerable<IEventValidator> validators;
        private readonly IOptions<AuthOptions> auth;

        public EventMessageHandler(
            ILogger<EventMessageHandler> logger,
            IEventDispatcher eventDispatcher,
            IEnumerable<IEventValidator> validators,
            IOptions<AuthOptions> auth)
        {
            this.logger = logger;
            this.eventDispatcher = eventDispatcher;
            this.validators = validators;
            this.auth = auth;
        }

        public bool CanHandleMessage(string type) => type == MessageType.Event;

        public async Task HandleMessageAsync(IWebSocketAdapter sender, JsonDocument[] parameters)
        {
            var e = EventParser.TryParse(parameters, out var ex);

            if (e == null)
            {
                this.logger.LogError(ex, $"Couldn't parse event: {parameters.ToString()}");
                throw new MessageProcessingException(Messages.ErrorProcessingEvent);
            }

            var auth = this.auth.Value.Mode;

            if (!sender.Context.IsAuthenticated() && (auth == AuthMode.Always || auth == AuthMode.Publishing))
            {
                this.logger.LogError("Auth required but client not authenticated");
                throw new MessageProcessingException(e, auth == AuthMode.Always ? Messages.AuthRequired : Messages.AuthRequiredPublishing);
            }

            var validation = this.validators.ValidateEvent(e);

            if (validation != null)
            {
                this.logger.LogError($"Couldn't validate event: {e.ToStringUnique()}");
                throw new MessageProcessingException(e, validation);
            }

            await this.eventDispatcher.DispatchEventAsync(sender, e);
        }
    }
}
