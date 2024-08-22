using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Netstr.Options;
using Netstr.Messaging.Models;
using System.Collections.Immutable;
using DotNext.Threading;

namespace Netstr.Messaging.WebSockets
{
    public class WebSocketAdapter : IWebSocketListenerAdapter, IWebSocketAdapter, IWebSocketSubscriptionsAdapter
    {
        private readonly ILogger<WebSocketAdapter> logger;
        private readonly IOptions<ConnectionOptions> connection;
        private readonly IOptions<LimitsOptions> limits;
        private readonly IMessageDispatcher dispatcher;
        private readonly WebSocket ws;
        private readonly Dictionary<string, Subscription> subscriptions;
        private readonly AsyncReaderWriterLock asyncLock;

        private CancellationToken cancellationToken;

        public WebSocketAdapter(
            ILogger<WebSocketAdapter> logger,
            IOptions<ConnectionOptions> connection,
            IOptions<LimitsOptions> limits,
            IMessageDispatcher dispatcher,
            CancellationToken cancellationToken,
            WebSocket ws,
            IHeaderDictionary headers)
        {
            this.logger = logger;
            this.connection = connection;
            this.limits = limits;
            this.dispatcher = dispatcher;
            this.cancellationToken = cancellationToken;
            this.ws = ws;
            this.subscriptions = new();
            this.asyncLock = new();

            ClientId = headers["sec-websocket-key"].ToString();
        }

        public string ClientId { get; }

        public void AddSubscription(string id, IEnumerable<SubscriptionFilter> filters)
        {
            this.logger.LogInformation(this.subscriptions.ContainsKey(id)
                ? $"Adding a new subscription {id} for client {ClientId}"
                : $"Replacing existing subscription {id} for client {ClientId}");
            this.subscriptions[id] = new Subscription(filters.ToArray(), DateTimeOffset.UtcNow);
        }

        public IDictionary<string, Subscription> GetSubscriptions()
        {
            return this.subscriptions.ToImmutableDictionary(x => x.Key, x => x.Value);
        }

        public async Task LockAsync(LockType type, Func<IWebSocketSubscriptionsAdapter, Task> func)
        {
            var lockHolder = type switch
            {
                LockType.Read => this.asyncLock.AcquireReadLockAsync(),
                LockType.Write => this.asyncLock.AcquireWriteLockAsync(),
                _ => throw new NotImplementedException()
            };

            using (await lockHolder)
            {
                await func(this);
            }
        }

        public void RemoveSubscription(string id)
        {
            this.logger.LogInformation($"Removing subscription {id} for client {ClientId}");
            this.subscriptions.Remove(id);
        }

        public Task SendAsync(object[] message)
        {
            return this.ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message), WebSocketMessageType.Text, true, this.cancellationToken);
        }

        public async Task StartAsync()
        {
            try
            {
                await ReceiveAsync(this.cancellationToken);
            }
            finally
            {
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
                    //                    stream.Write(buffer.Array, buffer.Offset, result.Count);
#pragma warning restore CS8604 // Possible null reference argument.


                    //                    stream.Seek(0, SeekOrigin.Begin);
                    //                    var message = await reader.ReadToEndAsync();

                    await this.dispatcher.DispatchMessageAsync(this, message);

                }
                catch (WebSocketException e)
                {
                    this.logger.LogError(e, "WebSocket exception");

                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        this.ws.Abort();
                    }
                }
            }
        }
    }
}
