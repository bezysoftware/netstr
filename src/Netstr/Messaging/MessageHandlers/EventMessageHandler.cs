using Microsoft.Extensions.Options;
using Netstr.Messaging.Events;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;
using System.Threading.RateLimiting;

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
        private readonly PartitionedRateLimiter<string> rateLimiter;

        public EventMessageHandler(
            ILogger<EventMessageHandler> logger,
            IEventDispatcher eventDispatcher,
            IEnumerable<IEventValidator> validators,
            IOptions<AuthOptions> auth,
            IOptions<LimitsOptions> limits
            )
        {
            this.logger = logger;
            this.eventDispatcher = eventDispatcher;
            this.validators = validators;
            this.auth = auth;
            this.rateLimiter = PartitionedRateLimiter.Create<string, string>(
                x => RateLimitPartition.GetSlidingWindowLimiter(x, _ => new SlidingWindowRateLimiterOptions 
                {
                    AutoReplenishment = true,
                    PermitLimit = limits.Value.Events.MaxEventsPerMinute > 0 ? limits.Value.Events.MaxEventsPerMinute : int.MaxValue,
                    SegmentsPerWindow = 6,
                    Window = TimeSpan.FromMinutes(1)
                }));
        }

        public bool CanHandleMessage(string type) => type == MessageType.Event;

        public async Task HandleMessageAsync(IWebSocketAdapter sender, JsonDocument[] parameters)
        {
            var e = EventParser.TryParse(parameters, out var ex);

            if (e == null)
            {
                this.logger.LogError(ex, $"Couldn't parse event: {parameters}");
                throw new UnknownMessageProcessingException(Messages.ErrorProcessingEvent);
            }

            using var lease = this.rateLimiter.AttemptAcquire(sender.Context.IpAddress);

            if (!lease.IsAcquired)
            {
                this.logger.LogInformation($"User {sender.Context.IpAddress} is rate limited");
                sender.SendNotOk(e.Id, Messages.RateLimited);
                return;
            }

            var auth = this.auth.Value.Mode;

            if (!sender.Context.IsAuthenticated() && (auth == AuthMode.Always || auth == AuthMode.Publishing))
            {
                this.logger.LogError("Auth required but client not authenticated");
                throw new EventProcessingException(e, auth == AuthMode.Always ? Messages.AuthRequired : Messages.AuthRequiredPublishing);
            }

            var validation = this.validators.ValidateEvent(e, sender.Context);

            if (validation != null)
            {
                this.logger.LogError($"Couldn't validate event: {e.ToStringUnique()}");
                throw new EventProcessingException(e, validation);
            }

            await this.eventDispatcher.DispatchEventAsync(sender, e);
        }
    }
}
