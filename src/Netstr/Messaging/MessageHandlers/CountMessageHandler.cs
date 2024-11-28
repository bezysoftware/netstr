using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions.Validators;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes COUNT messages.
    /// </summary>
    public class CountMessageHandler : FilterMessageHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public CountMessageHandler(
            IDbContextFactory<NetstrDbContext> db,
            IEnumerable<ISubscriptionRequestValidator> validators, 
            IOptions<LimitsOptions> limits, 
            IOptions<AuthOptions> auth,
            ILogger<CountMessageHandler> logger) 
            : base(validators, limits, auth, logger)
        {
            this.db = db;

        }

        protected override string AcceptedMessageType => MessageType.Count;

        protected override async Task HandleMessageCoreAsync(
            IWebSocketAdapter adapter, 
            string subscriptionId, 
            IEnumerable<SubscriptionFilter> filters,
            IEnumerable<JsonDocument> remainingParameters)
        {
            using var context = this.db.CreateDbContext();

            // get stored events count
            var count = await GetFilteredEvents(context, filters, adapter.Context.PublicKey).CountAsync();

            // send count back
            await adapter.SendCountAsync(subscriptionId, count);
        }
    }
}
