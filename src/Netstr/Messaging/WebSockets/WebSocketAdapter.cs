using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Options;
using Netstr.Options;
using Netstr.Messaging.Models;
using System.Threading.Channels;
using Netstr.Messaging.Subscriptions;
using System.Text.Json;
using Netstr.Messaging.Negentropy;

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
        private readonly Channel<MessageBatch> sendChannel;
        private CancellationToken cancellationToken;

        public WebSocketAdapter(
            ILogger<WebSocketAdapter> logger,
            IOptions<ConnectionOptions> connection,
            IOptions<LimitsOptions> limits,
            IOptions<AuthOptions> auth,
            IMessageDispatcher dispatcher,
            INegentropyAdapterFactory negentropyFactory,
            ISubscriptionsAdapterFactory subscriptionsFactory,
            CancellationToken cancellationToken,
            WebSocket ws,
            IHeaderDictionary headers,
            ConnectionInfo connectionInfo)
        {
            this.logger = logger;
            this.connection = connection;
            this.limits = limits;
            this.auth = auth;
            this.dispatcher = dispatcher;
            this.cancellationToken = cancellationToken;
            this.ws = ws;
            this.sendChannel = Channel.CreateBounded<MessageBatch>(
                new BoundedChannelOptions(limits.Value.MaxPendingEvents) { FullMode = BoundedChannelFullMode.DropOldest }, 
                e => logger.LogWarning($"Dropping following events due to capacity limit of {limits.Value.MaxPendingEvents}: {JsonSerializer.Serialize(e.Messages)}"));

            var id = headers["sec-websocket-key"].ToString();

            Context = new ClientContext(id, connectionInfo.RemoteIpAddress?.ToString() ?? string.Empty);
            
            Subscriptions = subscriptionsFactory.CreateAdapter(this);
            Negentropy = negentropyFactory.CreateAdapter(this);
        }

        public ClientContext Context { get; }

        public ISubscriptionsAdapter Subscriptions { get; }
        
        public INegentropyAdapter Negentropy { get; }

        public async Task SendAsync(MessageBatch batch)
        {
            await this.sendChannel.Writer.WriteAsync(batch);
        }

        public void Send(MessageBatch batch)
        {
            this.sendChannel.Writer.TryWrite(batch);
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
                
                Subscriptions.Dispose();
                Negentropy.Dispose();
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
                        this.logger.LogInformation($"Batch '{batch.Id}' closed mid-flight, stopping it");
                        break;
                    }

                    try
                    {
                        await this.ws.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
                    }
                    catch (WebSocketException ex)
                    {
                        this.logger.LogWarning(ex, $"WebSocket exception in SendAsync, ClientId: {this.Context.ClientId}");
                    }
                }
            }
        }
    }
}
