using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Options;
using Netstr.Options;
using Netstr.Messaging.Models;
using System.Collections.Immutable;
using System.Threading.Channels;
using Netstr.Messaging.Subscriptions;
using System.Text.Json;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapter : IWebSocketListenerAdapter, IWebSocketAdapter
    {
        private readonly ILogger<WebSocketAdapter> logger;
        private readonly IOptions<ConnectionOptions> connection;
        private readonly IOptions<LimitsOptions> limits;
        private readonly IOptions<AuthOptions> auth;
        private readonly IMessageDispatcher dispatcher;
        private readonly WebSocket ws;
        private readonly Dictionary<string, SubscriptionAdapter> subscriptions;
        private readonly Channel<MessageBatch> sendChannel;
        private CancellationToken cancellationToken;

        public WebSocketAdapter(
            ILogger<WebSocketAdapter> logger,
            IOptions<ConnectionOptions> connection,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth,
            IMessageDispatcher dispatcher,
            CancellationToken cancellationToken,
            WebSocket ws,
            IHeaderDictionary headers)
        {
            this.logger = logger;
            this.connection = connection;
            this.limits = limits;
            this.auth = auth;
            this.dispatcher = dispatcher;
            this.cancellationToken = cancellationToken;
            this.ws = ws;
            this.subscriptions = new();
            this.sendChannel = Channel.CreateBounded<MessageBatch>(
                new BoundedChannelOptions(limits.Value.MaxPendingEvents) { FullMode = BoundedChannelFullMode.DropOldest }, 
                e => logger.LogWarning($"Dropping following events due to capacity limit of {limits.Value.MaxPendingEvents}: {JsonSerializer.Serialize(e.Messages)}"));

            var id = headers["sec-websocket-key"].ToString();

            Context = new ClientContext(id);
        }

        public ClientContext Context { get; }

        public SubscriptionAdapter AddSubscription(string id, IEnumerable<SubscriptionFilter> filters)
        {
            if (this.subscriptions.TryGetValue(id, out var existing))
            {
                this.logger.LogInformation($"Disposing existing subscription {id} for client {Context.ClientId}");
                existing.Dispose();
            }

            return this.subscriptions[id] = new SubscriptionAdapter(this, id, filters.ToArray());
        }

        public IDictionary<string, SubscriptionAdapter> GetSubscriptions()
        {
            return this.subscriptions.ToImmutableDictionary(x => x.Key, x => x.Value);
        }

        public void RemoveSubscription(string id)
        {
            this.logger.LogInformation($"Removing subscription {id} for client {Context.ClientId}");
            this.subscriptions.Remove(id);
        }

        public async Task SendAsync(MessageBatch batch)
        {
            await this.sendChannel.Writer.WriteAsync(batch);
        }

        public async Task StartAsync()
        {
            try
            {
                // send auth challenge when it's not disabled
                if (this.auth.Value.Mode != AuthMode.Disabled)
                {
                    await this.SendAuthAsync(Context.Challenge);
                }

                // start sending & receiving messages
                await Task.WhenAny([
                    ReceiveAsync(this.cancellationToken),
                    SendAsync(this.cancellationToken)
                ]);
            }
            finally
            {
                this.sendChannel.Writer.Complete();
                this.subscriptions.Clear();
            }
        }

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            while (this.ws.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[this.limits.Value.MaxPayloadSize]);

                try
                {
                    using var stream = new MemoryStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    
                    var result = await this.ws.ReceiveAsync(buffer, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    if (!result.EndOfMessage)
                    {
                        // payload too large, disconnect
                        await this.SendNoticeAsync(Messages.InvalidPayloadTooLarge);
                        await this.ws.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, Messages.InvalidPayloadTooLarge, CancellationToken.None);
                        break;
                    }

#pragma warning disable CS8604 // Possible null reference argument.
                    var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
#pragma warning restore CS8604 // Possible null reference argument.

                    await this.dispatcher.DispatchMessageAsync(this, message);

                }
                catch (WebSocketException e)
                {
                    this.logger.LogError(e, $"WebSocket exception in ReceiveAsync, ClientId: {this.Context.ClientId}");

                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        this.ws.Abort();
                    }
                }
            }
        }

        private async Task SendAsync(CancellationToken cancellationToken)
        {
            while (this.ws.State == WebSocketState.Open)
            {
                var batch = await this.sendChannel.Reader.ReadAsync(cancellationToken);

                foreach (var message in batch.Messages)
                {
                    if (batch.IsCancelled)
                    {
                        break;
                    }

                    try
                    {
                        await this.ws.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
                    }
                    catch (WebSocketException ex)
                    {
                        this.logger.LogWarning(ex, $"WebSocket exception in SemdAsync, ClientId: {this.Context.ClientId}");
                    }
                }
            }
        }
    }
}
