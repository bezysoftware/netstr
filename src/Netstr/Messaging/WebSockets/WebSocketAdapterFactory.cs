using Microsoft.Extensions.Options;
using Netstr.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapterFactory
    {
        private readonly ILogger<WebSocketAdapter> logger;
        private readonly IOptions<ConnectionOptions> options;
        private readonly IMessageDispatcher dispatcher;
        private readonly IWebSocketAdapterCollection tracker;
        private readonly IHostApplicationLifetime lifetime;

        public WebSocketAdapterFactory(
            ILogger<WebSocketAdapter> logger,
            IOptions<ConnectionOptions> options,
            IMessageDispatcher dispatcher,
            IWebSocketAdapterCollection tracker,
            IHostApplicationLifetime lifetime)
        {
            this.logger = logger;
            this.options = options;
            this.dispatcher = dispatcher;
            this.tracker = tracker;
            this.lifetime = lifetime;
        }

        public IWebSocketListenerAdapter CreateAdapter(WebSocket socket, IHeaderDictionary headers)
        {
            var adapter = new WebSocketAdapter(
                this.logger,
                this.options,
                this.dispatcher,
                this.lifetime.ApplicationStopping,
                socket,
                headers);

            this.tracker.Add(adapter);

            return adapter;
        }

        public void DisposeAdapter(string id)
        {
            this.tracker.Remove(id);
        }
    }
}
