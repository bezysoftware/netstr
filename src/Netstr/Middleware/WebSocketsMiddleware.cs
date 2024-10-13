using Microsoft.Extensions.Options;
using Netstr.Messaging.WebSockets;
using Netstr.Options;

namespace Netstr.Middleware
{
    /// <summary>
    /// Accepts websocket connections and starts listening to messages.
    /// </summary>
    public class WebSocketsMiddleware
    {
        private readonly IOptions<ConnectionOptions> options;
        private readonly ILogger<WebSocketsMiddleware> logger;
        private readonly WebSocketAdapterFactory factory;
        private readonly RequestDelegate next;

        public WebSocketsMiddleware(
            IOptions<ConnectionOptions> options, 
            ILogger<WebSocketsMiddleware> logger,
            WebSocketAdapterFactory factory,
            RequestDelegate next)
        {
            this.options = options;
            this.logger = logger;
            this.factory = factory;
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == this.options.Value.WebSocketsPath && context.WebSockets.IsWebSocketRequest)
            {
                this.logger.LogInformation($"Accepting websocket connection from {context.Connection.RemoteIpAddress}");

                var ws = await context.WebSockets.AcceptWebSocketAsync();
                var adapter = this.factory.CreateAdapter(ws, context.Request.Headers, context.Connection);

                await adapter.StartAsync();

                this.logger.LogInformation($"Closing websocket connection from {context.Connection.RemoteIpAddress}");
                this.factory.DisposeAdapter(adapter.Context.ClientId);
            }
            else
            {
                await this.next(context);
            }
        }
    }
}
