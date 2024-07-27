using System.Collections.Concurrent;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapterCollection : IWebSocketAdapterCollection
    {
        private readonly ConcurrentDictionary<string, IWebSocketAdapter> adapters;

        public WebSocketAdapterCollection()
        {
            this.adapters = new();
        }

        public void Remove(string id)
        {
            this.adapters.TryRemove(id, out var _);
        }

        public void Add(IWebSocketAdapter adapter)
        {
            this.adapters.TryAdd(adapter.ClientId, adapter);
        }

        public IEnumerable<IWebSocketAdapter> GetAll()
        {
            return this.adapters.Values.ToArray();
        }
    }
}
