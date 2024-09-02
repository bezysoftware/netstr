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

        protected override async Task HandleMessageCoreAsync(DateTimeOffset processingStart, IWebSocketAdapter adapter, string subscriptionId, IEnumerable<SubscriptionFilter> filters)
        {
            // lock to make sure incoming events will have to wait until EOSE is sent
            await adapter.LockAsync(LockType.Write, async x =>
            {
                var maxSubscriptions = this.limits.Value.MaxSubscriptions;
                if (maxSubscriptions > 0 && x.GetSubscriptions().Where(x => x.Key != subscriptionId).Count() >= maxSubscriptions)
                {
                    throw new MessageProcessingException(subscriptionId, Messages.InvalidTooManySubscriptions);
                }

                using var context = this.db.CreateDbContext();

                // get stored events
                var entities = await GetFilteredEvents(context, filters, adapter.Context.PublicKey, processingStart).ToArrayAsync();
                var events = entities.Select(CreateEvent).ToArray();

                // add sub
                x.AddSubscription(subscriptionId, filters);

                // send back
                await adapter.SendEventsAsync(subscriptionId, events);

                // EOSE
                await x.SendEndOfStoredEventsAsync(subscriptionId);
            });
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
