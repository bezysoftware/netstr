using Microsoft.Extensions.Options;
using Netstr.Messaging.Negentropy;
using Netstr.Messaging.Subscriptions;
using Netstr.Options;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapterFactory
    {
        private readonly ILogger<WebSocketAdapter> logger;
        private readonly IOptions<LimitsOptions> limits;
        private readonly IOptions<AuthOptions> auth;
        private readonly IMessageDispatcher dispatcher;
        private readonly IWebSocketAdapterCollection tracker;
        private readonly IHostApplicationLifetime lifetime;
        private readonly INegentropyAdapterFactory negentropyFactory;
        private readonly ISubscriptionsAdapterFactory subscriptionsFactory;

        public WebSocketAdapterFactory(
            ILogger<WebSocketAdapter> logger,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth,
            IMessageDispatcher dispatcher,
            IWebSocketAdapterCollection tracker,
            IHostApplicationLifetime lifetime,
            INegentropyAdapterFactory negentropyFactory,
            ISubscriptionsAdapterFactory subscriptionsFactory)
        {
            this.logger = logger;
            this.limits = limits;
            this.auth = auth;
            this.dispatcher = dispatcher;
            this.tracker = tracker;
            this.lifetime = lifetime;
            this.negentropyFactory = negentropyFactory;
            this.subscriptionsFactory = subscriptionsFactory;
        }

        public IWebSocketListenerAdapter CreateAdapter(WebSocket socket, IHeaderDictionary headers, ConnectionInfo connection)
        {
            var adapter = new WebSocketAdapter(
                this.logger,
                this.limits,
                this.auth,
                this.dispatcher,
                this.negentropyFactory,
                this.subscriptionsFactory,
                this.lifetime.ApplicationStopping,
                socket,
                headers,
                connection);

            this.tracker.Add(adapter);

            return adapter;
        }

        public void DisposeAdapter(string id)
        {
            this.tracker.Remove(id);
        }
    }
}
