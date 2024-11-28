using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Json;
using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;
using Netstr.Options.Limits;
using System.ComponentModel;
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
                x => RateLimitPartition.GetSlidingWindowLimiter(x, _ => {
                    var limits = GetLimits();
                    return new SlidingWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = limits.MaxSubscriptionsPerMinute > 0 ? limits.MaxSubscriptionsPerMinute : int.MaxValue,
                        SegmentsPerWindow = 6,
                        Window = TimeSpan.FromMinutes(1)
                    };
                }));
        }

        protected abstract string AcceptedMessageType { get; }

        protected virtual bool SingleFilter { get; }

        public bool CanHandleMessage(string type) => AcceptedMessageType == type;

        public async Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new UnknownMessageProcessingException($"{AcceptedMessageType} message should be an array with at least 2 elements");
            }

            var id = parameters[1].DeserializeRequired<string>();
            using var lease = this.rateLimiter.AttemptAcquire(adapter.Context.IpAddress);

            if (!lease.IsAcquired)
            {
                RaiseSubscriptionException(id, Messages.RateLimited, $"User {adapter.Context.IpAddress} is rate limited");
            }

            if (this.auth.Value.Mode == AuthMode.Always && !adapter.Context.IsAuthenticated())
            {
                RaiseSubscriptionException(id, Messages.AuthRequired);
            }

            // limit number of filters, pass whatever follows the filter list to Core method (JsonDocument)
            var filters = parameters
                .Skip(2)
                .Take(SingleFilter ? 1 : int.MaxValue)
                .Select(x => GetSubscriptionFilter(id, x))
                .ToArray();

            var validationError = this.validators.CanSubscribe(id, adapter.Context, filters, this);
            if (validationError != null)
            {
                RaiseSubscriptionException(id, validationError);
            }

            await HandleMessageCoreAsync(adapter, id, filters, parameters.Skip(filters.Length + 2).ToArray());
        }

        protected abstract Task HandleMessageCoreAsync(
            IWebSocketAdapter adapter, 
            string subscriptionId, 
            IEnumerable<SubscriptionFilter> filters,
            IEnumerable<JsonDocument> remainingParameters);

        protected virtual SubscriptionLimits GetLimits()
        {
            return this.limits.Value.Subscriptions;
        }

        protected IQueryable<EventEntity> GetFilteredEvents(NetstrDbContext db, IEnumerable<SubscriptionFilter> filters, string? clientPublicKey)
        {
            // if auth is disabled ignore any set ProtectedKinds
            var auth = this.auth.Value;
            var protectedKinds = auth.Mode == AuthMode.Disabled ? [] : auth.ProtectedKinds;
            var now = DateTimeOffset.UtcNow;
            var limits = GetLimits();

            return db.Events
                .WhereAnyFilterMatches(filters, protectedKinds, clientPublicKey, limits.MaxInitialLimit)
                .Where(x =>
                    !x.DeletedAt.HasValue &&
                    (!x.EventExpiration.HasValue || x.EventExpiration.Value > now))
                .OrderByDescending(x => x.EventCreatedAt)
                .ThenBy(x => x.EventId);
        }

        protected virtual void RaiseSubscriptionException(string subscriptionId, string message, string? logMessage = null)
        {
            throw new SubscriptionProcessingException(subscriptionId, message, logMessage);
        }

        private SubscriptionFilter GetSubscriptionFilter(string subscriptionId, JsonDocument json)
        {
            var r = DeserializeFilter(subscriptionId, json);

            // only single letter tags with AND and OR modifiers are allowed as tag filters
            if (r.AdditionalData?.Any(x => (!x.Key.StartsWith(TagModifierOr) && !x.Key.StartsWith(TagModifierAnd)) || x.Key.Length != 2) ?? false)
            {
                RaiseSubscriptionException(subscriptionId, Messages.UnsupportedFilter);
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

        private SubscriptionFilterRequest DeserializeFilter(string subscriptionId, JsonDocument json)
        {
            try
            {
                return json.DeserializeRequired<SubscriptionFilterRequest>();
            }
            catch(Exception ex)
            {
                RaiseSubscriptionException(subscriptionId, Messages.InvalidCannotProcessFilters, ex.Message);
                throw;
            }
        }
    }
}
