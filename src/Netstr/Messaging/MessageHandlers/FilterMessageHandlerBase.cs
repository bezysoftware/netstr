using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Json;
using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Base class for all filter-based messages. E.g. REQ message and COUNT message.
    /// </summary>
    public abstract class FilterMessageHandlerBase : IMessageHandler
    {
        const char TagModifierOr = '#';
        const char TagModifierAnd = '&';

        protected readonly IEnumerable<ISubscriptionRequestValidator> validators;
        protected readonly IOptions<LimitsOptions> limits;
        protected readonly IOptions<AuthOptions> auth;
        protected readonly ILogger<FilterMessageHandlerBase> logger;
        protected readonly PartitionedRateLimiter<string> rateLimiter;

        protected FilterMessageHandlerBase(
            IEnumerable<ISubscriptionRequestValidator> validators,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth,
            ILogger<FilterMessageHandlerBase> logger)
        {
            this.validators = validators;
            this.limits = limits;
            this.auth = auth;
            this.logger = logger;
            this.rateLimiter = PartitionedRateLimiter.Create<string, string>(
                x => RateLimitPartition.GetSlidingWindowLimiter(x, _ => new SlidingWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = limits.Value.MaxSubscriptionsPerMinute > 0 ? limits.Value.MaxSubscriptionsPerMinute : int.MaxValue,
                    SegmentsPerWindow = 6,
                    Window = TimeSpan.FromMinutes(1)
                }));
        }

        protected abstract string AcceptedMessageType { get; }

        public bool CanHandleMessage(string type) => AcceptedMessageType == type;

        public async Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new MessageProcessingException($"{AcceptedMessageType} message should be an array with at least 2 elements");
            }

            var id = parameters[1].DeserializeRequired<string>();
            using var lease = this.rateLimiter.AttemptAcquire(adapter.Context.IpAddress);

            if (!lease.IsAcquired)
            {
                this.logger.LogInformation($"User {adapter.Context.IpAddress} is rate limited");
                await adapter.SendClosedAsync(id, Messages.RateLimited);
                return;
            }

            if (this.auth.Value.Mode == AuthMode.Always && !adapter.Context.IsAuthenticated())
            {
                throw new MessageProcessingException(id, Messages.AuthRequired);
            }

            var filters = parameters
                .Skip(2)
                .Select(x => GetSubscriptionFilter(id, x))
                .ToArray();

            var validationError = this.validators.CanSubscribe(id, adapter.Context, filters);
            if (validationError != null)
            {
                throw new MessageProcessingException(id, validationError);
            }

            await HandleMessageCoreAsync(adapter, id, filters);
        }

        protected abstract Task HandleMessageCoreAsync(IWebSocketAdapter adapter, string subscriptionId, IEnumerable<SubscriptionFilter> filters);

        protected IQueryable<EventEntity> GetFilteredEvents(NetstrDbContext db, IEnumerable<SubscriptionFilter> filters, string? clientPublicKey)
        {
            // if auth is disabled ignore any set ProtectedKinds
            var auth = this.auth.Value;
            var protectedKinds = auth.Mode == AuthMode.Disabled ? [] : auth.ProtectedKinds;
            var now = DateTimeOffset.UtcNow;

            return db.Events
                .WhereAnyFilterMatches(filters, protectedKinds, clientPublicKey, this.limits.Value.MaxInitialLimit)
                .Where(x =>
                    !x.DeletedAt.HasValue &&
                    (!x.EventExpiration.HasValue || x.EventExpiration.Value > now))
                .OrderByDescending(x => x.EventCreatedAt)
                .ThenBy(x => x.EventId);
        }

        private SubscriptionFilter GetSubscriptionFilter(string subscriptionId, JsonDocument json)
        {
            var r = json.DeserializeRequired<SubscriptionFilterRequest>();

            // only single letter tags with AND and OR modifiers are allowed as tag filters
            if (r.AdditionalData?.Any(x => (!x.Key.StartsWith(TagModifierOr) && !x.Key.StartsWith(TagModifierAnd)) || x.Key.Length != 2) ?? false)
            {
                throw new MessageProcessingException(subscriptionId, Messages.UnsupportedFilter);
            }

            Func<Dictionary<string, JsonElement>?, char, Dictionary<string, string[]>> getTags = (data, type) => data?
                .Where(x => x.Key.StartsWith(type))
                .ToDictionary(x => x.Key.TrimStart(type), x => x.Value.DeserializeRequired<string[]>())
                ?? new();

            var orTags = getTags(r.AdditionalData, TagModifierOr);
            var andTags = getTags(r.AdditionalData, TagModifierAnd);
            
            return new SubscriptionFilter(
                r.Ids.EmptyIfNull(),
                r.Authors.EmptyIfNull(),
                r.Kinds.EmptyIfNull(),
                r.Since,
                r.Until,
                r.Limit,
                orTags,
                andTags);
        }
    }
}
