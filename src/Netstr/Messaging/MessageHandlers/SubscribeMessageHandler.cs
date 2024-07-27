using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Messaging.Matching;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes REQ messages.
    /// </summary>
    public class SubscribeMessageHandler : IMessageHandler
    {
        private readonly IDbContextFactory<NetstrDbContext> db;
        private readonly IOptions<LimitsOptions> limits;

        public SubscribeMessageHandler(IDbContextFactory<NetstrDbContext> db, IOptions<LimitsOptions> limits)
        {
            this.db = db;
            this.limits = limits;
        }

        public bool CanHandleMessage(string type) => type == MessageType.Req;

        public async Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new MessageProcessingException("REQ message should be an array with at least 2 elements");
            }

            var id = parameters[1].DeserializeRequired<string>();
            var filters = parameters
                .Skip(2)
                .Select(GetSubscriptionFilter)
                .ToArray();

            // lock to make sure incoming events will have to wait until EOSE is sent
            await adapter.LockAsync(LockType.Write, async x =>
            {
                using var context = this.db.CreateDbContext();

                // add sub
                x.AddSubscription(id, filters);

                // get stored events
                var entities = await context.Events.WhereAnyFilterMatches(filters, this.limits.Value.MaxInitialLimit).ToArrayAsync();
                var events = entities.Select(CreateEvent).ToArray();

                // send back
                await adapter.SendEventsAsync(id, events);

                // EOSE
                await x.SendEndOfStoredEventsAsync(id);
            });
        }

        private SubscriptionFilter GetSubscriptionFilter(JsonDocument json) 
        {
            var r = json.DeserializeRequired<SubscriptionFilterRequest>();
            var tags = r.AdditionalData?.ToDictionary(x => x.Key, x => x.Value.DeserializeRequired<string[]>()) ?? new();
            
            return new SubscriptionFilter(
                r.Ids.EmptyIfNull(), 
                r.Authors.EmptyIfNull(), 
                r.Kinds.EmptyIfNull(),
                r.Since, 
                r.Until, 
                r.Limit, 
                tags);
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
