using Microsoft.Extensions.Options;
using Negentropy;
using Netstr.Options;
using System.Collections.Concurrent;

namespace Netstr.Messaging.Negentropy
{
    public interface INegentropyAdapter : IDisposable
    {
        void Open(string subscriptionId, string query, IReadOnlyCollection<INegentropyItem> items);

        void Message(string subscriptionId, string query);

        void Close(string subscriptionId);

        IEnumerable<string> GetOpenSubscriptions();

        void DisposeStaleSubscriptions();
    }

    public class NegentropyAdapter : INegentropyAdapter
    {
        private readonly ConcurrentDictionary<string, NegentropySubscription> subscriptions;
        private readonly ILogger<NegentropyAdapter> logger;
        private readonly IWebSocketAdapter ws;
        private readonly IOptions<LimitsOptions> options;

        public NegentropyAdapter(ILogger<NegentropyAdapter> logger, IWebSocketAdapter webSocketAdapter, IOptions<LimitsOptions> options)
        {
            this.subscriptions = new();
            this.logger = logger;
            this.ws = webSocketAdapter;
            this.options = options;
        }

        public IEnumerable<string> GetOpenSubscriptions()
        {
            return this.subscriptions.Keys;
        }

        public void Close(string subscriptionId)
        {
            if (this.subscriptions.TryRemove(subscriptionId, out var subscription))
            {
                this.logger.LogInformation($"Closing negentropy subscription {subscriptionId} for {this.ws.Context}");
            }
            else
            {
                // no such subscription, do nothing
                this.logger.LogWarning($"Received a negentropy message for client {this.ws.Context} and unknown subscription {subscriptionId}");
            }
        }

        public void Message(string subscriptionId, string query)
        {
            if (this.subscriptions.TryGetValue(subscriptionId, out var n))
            {
                this.logger.LogInformation($"Processing negentropy message for {this.ws.Context}, subscription {subscriptionId}");
                
                var (q, _, _) = n.Reconcile(query);

                this.ws.SendNegentropyMessage(subscriptionId, q);
            }
            else
            {
                // no such subscription
                this.logger.LogWarning($"Received a negentropy message for client {this.ws.Context} and unknown subscription {subscriptionId}");
                this.ws.SendNegentropyError(subscriptionId, Messages.Negentropy.ClosedUnknownId);
            }
        }

        public void Open(string subscriptionId, string query, IReadOnlyCollection<INegentropyItem> items)
        {
            this.logger.LogInformation($"Starting negentropy for {this.ws.Context}, subscription {subscriptionId}, total items {items.Count}");
            
            var n = new NegentropySubscription(items, this.options.Value.Negentropy.FrameSizeLimit);

            this.subscriptions.AddOrUpdate(subscriptionId, n, (_, _) => n);

            var q = n.Reconcile(query).Query;

            this.ws.SendNegentropyMessage(subscriptionId, q);
        }

        public void DisposeStaleSubscriptions()
        {
            var absoluteCutoff = DateTimeOffset.UtcNow.AddSeconds(-this.options.Value.Negentropy.MaxSubscriptionAgeSeconds);
            var relativeCutoff = DateTimeOffset.UtcNow.AddSeconds(-this.options.Value.Negentropy.StaleSubscriptionLimitSeconds);
            var subs = this.subscriptions.ToArray();

            if (subs.Length > 0) 
            {
                this.logger.LogInformation($"Found {subs.Length} stale negentropy subscriptions, disposing them");
            }

            foreach (var subscription in subs)
            {
                if (relativeCutoff > subscription.Value.LastMessageOn || absoluteCutoff > subscription.Value.StartedOn)
                {
                    Close(subscription.Key);
                    this.ws.SendNegentropyError(subscription.Key, Messages.Negentropy.ClosedTimeout);
                }
            }
        }

        public void Dispose()
        {
            DisposeStaleSubscriptions();
        }
    }
}
