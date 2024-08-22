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

            var start = DateTimeOffset.UtcNow;

            var id = parameters[1].DeserializeRequired<string>();
            var filters = parameters
                .Skip(2)
                .Select(GetSubscriptionFilter)
                .ToArray();

            if (!CanSubscribe(id, filters, out var error))
            {
                await adapter.SendClosedAsync(id, error ?? "");
                return;
            }

            // lock to make sure incoming events will have to wait until EOSE is sent
            await adapter.LockAsync(LockType.Write, async x =>
            {
                var maxSubscriptions = this.limits.Value.MaxSubscriptions;
                if (maxSubscriptions > 0 && x.GetSubscriptions().Where(x => x.Key != id).Count() >= maxSubscriptions)
                {
                    await adapter.SendClosedAsync(id, Messages.InvalidTooManySubscriptions);
                    return;
                }

                using var context = this.db.CreateDbContext();

                // get stored events
                var entities = await context.Events
                    .WhereAnyFilterMatches(filters, this.limits.Value.MaxInitialLimit)
                    .Where(x => x.FirstSeen < start)
                    .Where(x => !x.DeletedAt.HasValue)
                    .OrderByDescending(x => x.EventCreatedAt)
                    .ThenBy(x => x.EventId)
                    .ToArrayAsync();

                var events = entities.Select(CreateEvent).ToArray();
                var maxFirstSeen = entities.MaxOrDefault(x => x.FirstSeen);

                // add sub
                x.AddSubscription(id, filters);

                // send back
                await adapter.SendEventsAsync(id, events);

                // EOSE
                await x.SendEndOfStoredEventsAsync(id);
            });
        }

        private bool CanSubscribe(string id, SubscriptionFilter[] filters, out string? error) 
        {
            error = null;
            var limits = this.limits.Value;

            if (limits.MaxSubscriptionIdLength > 0 && id.Length > limits.MaxSubscriptionIdLength)
            {
                error = Messages.InvalidSubscriptionIdTooLong;
            }
            else if (limits.MaxFilters > 0 && filters.Length > limits.MaxFilters)
            {
                error = Messages.InvalidTooManyFilters;
            }
            else if (limits.MaxInitialLimit > 0 && filters.Any(x => x.Limit > limits.MaxInitialLimit))
            {
                error = Messages.InvalidLimitTooHigh;
            }

            return error == null;
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
