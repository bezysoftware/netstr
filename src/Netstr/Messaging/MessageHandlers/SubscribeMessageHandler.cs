using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes REQ messages.
    /// </summary>
    public class SubscribeMessageHandler : FilterMessageHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public SubscribeMessageHandler(
            IDbContextFactory<NetstrDbContext> db,
            IEnumerable<ISubscriptionRequestValidator> validators,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth)
            : base(validators, limits, auth)
        {
            this.db = db;
        }

        protected override string AcceptedMessageType => MessageType.Req;

        protected override async Task HandleMessageCoreAsync(IWebSocketAdapter adapter, string subscriptionId, IEnumerable<SubscriptionFilter> filters)
        {
            var maxSubscriptions = this.limits.Value.MaxSubscriptions;
            if (maxSubscriptions > 0 && adapter.GetSubscriptions().Where(x => x.Key != subscriptionId).Count() >= maxSubscriptions)
            {
                throw new MessageProcessingException(subscriptionId, Messages.InvalidTooManySubscriptions);
            }

            using var context = this.db.CreateDbContext();

            // add sub
            var subscription = adapter.AddSubscription(subscriptionId, filters);

            // get stored events
            var entities = await GetFilteredEvents(context, filters, adapter.Context.PublicKey).ToArrayAsync();
            var events = entities.Select(CreateEvent).ToArray();

            // send stored events (also sends EOSE)
            await subscription.SendStoredEventsAsync(events);
        }

        private Event CreateEvent(EventEntity e)
        {
            return new Event
            {
                Id = e.EventId,
                Content = e.EventContent,
                CreatedAt = e.EventCreatedAt,
                Kind = e.EventKind,
                PublicKey = e.EventPublicKey,
                Signature = e.EventSignature,
                Tags = e.Tags.Select(tag => (string[])[ tag.Name, ..tag.Values ]).ToArray()
            };
        }
    }
}
