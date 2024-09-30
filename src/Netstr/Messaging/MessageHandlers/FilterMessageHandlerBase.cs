using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Base class for all filter-based messages. E.g. REQ message and COUNT message.
    /// </summary>
    public abstract class FilterMessageHandlerBase : IMessageHandler
    {
        protected readonly IEnumerable<ISubscriptionRequestValidator> validators;
        protected readonly IOptions<LimitsOptions> limits;
        protected readonly IOptions<AuthOptions> auth;

        protected FilterMessageHandlerBase(
            IEnumerable<ISubscriptionRequestValidator> validators,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth)
        {
            this.validators = validators;
            this.limits = limits;
            this.auth = auth;
        }

        protected abstract string AcceptedMessageType { get; }

        public bool CanHandleMessage(string type) => AcceptedMessageType == type;

        public async Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new MessageProcessingException($"{AcceptedMessageType} message should be an array with at least 2 elements");
            }

            var start = DateTimeOffset.UtcNow;
            var id = parameters[1].DeserializeRequired<string>();

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

            await HandleMessageCoreAsync(start, adapter, id, filters);
        }

        protected abstract Task HandleMessageCoreAsync(DateTimeOffset processingStart, IWebSocketAdapter adapter, string subscriptionId, IEnumerable<SubscriptionFilter> filters);

        protected IQueryable<EventEntity> GetFilteredEvents(NetstrDbContext db, IEnumerable<SubscriptionFilter> filters, string? clientPublicKey, DateTimeOffset start)
        {
            // if auth is disabled ignore any set ProtectedKinds
            var auth = this.auth.Value;
            var protectedKinds = auth.Mode == AuthMode.Disabled ? [] : auth.ProtectedKinds;

            return db.Events
                .WhereAnyFilterMatches(filters, protectedKinds, clientPublicKey, this.limits.Value.MaxInitialLimit)
                .Where(x =>
                    x.FirstSeen < start &&
                    !x.DeletedAt.HasValue &&
                    (!x.EventExpiration.HasValue || x.EventExpiration.Value > start))
                .OrderByDescending(x => x.EventCreatedAt)
                .ThenBy(x => x.EventId);
        }

        private SubscriptionFilter GetSubscriptionFilter(string subscriptionId, JsonDocument json)
        {
            var r = json.DeserializeRequired<SubscriptionFilterRequest>();

            if (r.AdditionalData?.Any(x => !x.Key.StartsWith("#") || x.Key.Length != 2) ?? false)
            {
                throw new MessageProcessingException(subscriptionId, Messages.UnsupportedFilter);
            }

            var tags = r.AdditionalData?.ToDictionary(x => x.Key.TrimStart('#'), x => x.Value.DeserializeRequired<string[]>()) ?? new();

            return new SubscriptionFilter(
                r.Ids.EmptyIfNull(),
                r.Authors.EmptyIfNull(),
                r.Kinds.EmptyIfNull(),
                r.Since,
                r.Until,
                r.Limit,
                tags);
        }
    }
}
