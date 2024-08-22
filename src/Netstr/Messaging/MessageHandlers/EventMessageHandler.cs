using Netstr.Extensions;
using Netstr.Messaging.Events;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.Models;
using System;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public EventMessageHandler(
            ILogger<EventMessageHandler> logger,
            IEventDispatcher eventDispatcher,
            IEnumerable<IEventValidator> validators)
        {
            this.logger = logger;
            this.eventDispatcher = eventDispatcher;
            this.validators = validators;
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

            var validation = ValidateEvent(e);

            if (validation != null)
            {
                this.logger.LogError(ex, $"Couldn't validate event: {e.ToStringUnique()}");
                throw new MessageProcessingException(e, validation);
            }

            await this.eventDispatcher.DispatchEventAsync(sender, e);
        }

        private string? ValidateEvent(Event e)
        {
            foreach (var validator in this.validators)
            {
                var error = validator.Validate(e);
                if (error != null)
                {
                    return error;
                }
            }

            return null;
        }
    }
}
