using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Json;
using Netstr.Messaging.Models;
using Netstr.Messaging.Negentropy;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers.Negentropy
{
    public class NegentropyOpenHandler : FilterMessageHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public NegentropyOpenHandler(
            IDbContextFactory<NetstrDbContext> db,
            IEnumerable<ISubscriptionRequestValidator> validators,
            IOptions<NegentropyLimitsOptions> limits,
            IOptions<AuthOptions> auth,
            ILogger<NegentropyOpenHandler> logger)
            : base(validators, limits, auth, logger)
        {
            this.db = db;
        }

        protected override string AcceptedMessageType => MessageType.Negentropy.Open;

        protected override bool SingleFilter => true;

        protected override async Task HandleMessageCoreAsync(
            IWebSocketAdapter adapter, 
            string subscriptionId, 
            IEnumerable<SubscriptionFilter> filters,
            IEnumerable<JsonDocument> remainingParameters)
        {
            var maxSubscriptions = this.limits.Value.MaxSubscriptions;
            if (maxSubscriptions > 0 && adapter.Negentropy.GetOpenSubscriptions().Where(x => x != subscriptionId).Count() >= maxSubscriptions)
            {
                adapter.SendNegentropyError(subscriptionId, Messages.InvalidTooManySubscriptions);
                return;
            }

            using var context = this.db.CreateDbContext();
            
            var query = remainingParameters.First().DeserializeRequired<string>();
            var events = await GetFilteredEvents(context, filters, adapter.Context.PublicKey)
                .Select(x => new NegentropyEvent(x.EventId, x.EventCreatedAt.UtcTicks))
                .ToArrayAsync();

            try
            {
                adapter.Negentropy.Open(subscriptionId, query, events);
            }
            catch (Exception ex)
            {
                throw new NegentropyProcessingException(subscriptionId, Messages.Negentropy.InvalidMessage, ex.Message);
            }
        }

        protected override void RaiseSubscriptionException(string subscriptionId, string message, string? logMessage = null)
        {
            throw new NegentropyProcessingException(subscriptionId, message, logMessage);
        }
    }
}
