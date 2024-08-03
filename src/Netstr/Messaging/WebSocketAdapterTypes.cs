using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    public interface IWebSocketListenerAdapter
    {
        Task StartAsync();
        string ClientId { get; }
    }

    public interface IWebSocketAdapter
    {
        Task LockAsync(LockType type, Func<IWebSocketSubscriptionsAdapter, Task> func);

        Task SendAsync(object[] message);

        string ClientId { get; }
    }

    public interface IWebSocketSubscriptionsAdapter : IWebSocketAdapter
    {
        void AddSubscription(string id, IEnumerable<SubscriptionFilter> filters);

        void RemoveSubscription(string id);

        IDictionary<string, Subscription> GetSubscriptions();
    }

    public interface IWebSocketAdapterCollection
    {
        void Add(IWebSocketAdapter adapter);

        IEnumerable<IWebSocketAdapter> GetAll();

        void Remove(string id);
    }


    public enum LockType
    {
        Read,
        Write
    }
}
