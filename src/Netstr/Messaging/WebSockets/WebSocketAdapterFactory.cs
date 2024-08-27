using Microsoft.Extensions.Options;
using Netstr.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapterFactory
    {
        private readonly ILogger<WebSocketAdapter> logger;
        private readonly IOptions<ConnectionOptions> connection;
        private readonly IOptions<LimitsOptions> limits;
        private readonly IOptions<AuthOptions> auth;
        private readonly IMessageDispatcher dispatcher;
        private readonly IWebSocketAdapterCollection tracker;
        private readonly IHostApplicationLifetime lifetime;

        public WebSocketAdapterFactory(
            ILogger<WebSocketAdapter> logger,
            IOptions<ConnectionOptions> connection,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth,
            IMessageDispatcher dispatcher,
            IWebSocketAdapterCollection tracker,
            IHostApplicationLifetime lifetime)
        {
            this.logger = logger;
            this.connection = connection;
            this.limits = limits;
            this.auth = auth;
            this.dispatcher = dispatcher;
            this.tracker = tracker;
            this.lifetime = lifetime;
        }

        public IWebSocketListenerAdapter CreateAdapter(WebSocket socket, IHeaderDictionary headers)
        {
            var adapter = new WebSocketAdapter(
                this.logger,
                this.connection,
                this.limits,
                this.auth,
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
