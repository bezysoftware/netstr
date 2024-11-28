using Netstr.Messaging.Models;
using Netstr.Messaging.Negentropy;
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

        void Send(MessageBatch batch);

        ISubscriptionsAdapter Subscriptions { get; }
        
        INegentropyAdapter Negentropy { get; }

        ClientContext Context { get; }
    }

    public interface IWebSocketAdapterCollection
    {
        void Add(IWebSocketAdapter adapter);

        IEnumerable<IWebSocketAdapter> GetAll();

        void Remove(string id);
    }
}
