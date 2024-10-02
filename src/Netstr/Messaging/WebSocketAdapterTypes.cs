using Netstr.Messaging.Models;
using Netstr.Messaging.Subscriptions;

namespace Netstr.Messaging
{
    public interface IWebSocketListenerAdapter
    {
        Task StartAsync();
        ClientContext Context { get; }
    }

    public interface IWebSocketAdapter
    {
        Task SendAsync(MessageBatch batch);

        SubscriptionAdapter AddSubscription(string id, IEnumerable<SubscriptionFilter> filters);

        void RemoveSubscription(string id);

        IDictionary<string, SubscriptionAdapter> GetSubscriptions();

        ClientContext Context { get; }
    }

    public interface IWebSocketAdapterCollection
    {
        void Add(IWebSocketAdapter adapter);

        IEnumerable<IWebSocketAdapter> GetAll();

        void Remove(string id);
    }
}
