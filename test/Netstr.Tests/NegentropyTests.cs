using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Negentropy;
using Netstr.Data;
using Netstr.Messaging;
using Netstr.Options;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace Netstr.Tests
{
    public class NegentropyTests
    {
        private WebApplicationFactory factory;

        public NegentropyTests()
        {
            this.factory = new WebApplicationFactory();
            this.factory.NegentropyLimits = new NegentropyLimitsOptions
            {
                MaxPayloadSize = 4096,
                MaxEventTags = 2,
                MaxInitialLimit = 20000,
                MaxFilters = 2,
                MaxSubscriptionIdLength = 5,
                MaxSubscriptions = 1, 
                StaleSubscriptionLimitSeconds = 1,
                StaleSubscriptionPeriodSeconds = 1
            };
        }

        [Fact]
        public async Task InvalidPayloadTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            await ws.SendAsync([
                "NEG-OPEN",
                "test",
                ""
            ]);

            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("test");
            received[2].GetString().Should().Be(Messages.InvalidCannotProcessFilters);
        }

        [Fact]
        public async Task InvalidMessageTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            await ws.SendNegentropyOpenAsync("test", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, "");

            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("test");
            received[2].GetString().Should().Be(Messages.Negentropy.InvalidMessage);
        }

        [Fact]
        public async Task SubscriptionIdTooLongTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            await ws.SendNegentropyOpenAsync("abcdabcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, "");

            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("abcdabcd");
            received[2].GetString().Should().Be(Messages.InvalidSubscriptionIdTooLong);
        }

        [Fact]
        public async Task SyncTimeoutTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, msg);

            var cts = new CancellationTokenSource(2000);
            var received = await ws.ReceiveOnceAsync(cts.Token);

            received[0].GetString().Should().Be("NEG-MSG");
            received[1].GetString().Should().Be("abcd");

            cts = new CancellationTokenSource(2000);

            received = await ws.ReceiveOnceAsync(cts.Token);

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("abcd");
            received[2].GetString().Should().Be(Messages.Negentropy.ClosedTimeout);
        }

        [Fact]
        public async Task SyncDoesntTimeoutTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, msg);
            await Task.Delay(800);
            
            await ws.SendNegentropyMessageAsync("abcd", msg);
            await Task.Delay(800);

            var cts = new CancellationTokenSource(2000);

            var received = await ws.ReceiveOnceAsync(cts.Token);

            received[0].GetString().Should().Be("NEG-MSG");
            received[1].GetString().Should().Be("abcd");
        }

        [Fact]
        public async Task SubscriptionWithSameIdIsRestartedTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, msg);
            await ws.ReceiveOnceAsync();
            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [1] }, msg);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-MSG");
            received[1].GetString().Should().Be("abcd");
        }

        [Fact]
        public async Task TooManyActiveSubscriptionsTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [0] }, msg);
            await ws.ReceiveOnceAsync();
            
            await ws.SendNegentropyOpenAsync("efgh", new Messaging.Models.SubscriptionFilterRequest { Kinds = [1] }, msg);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("efgh");
            received[2].GetString().Should().Be(Messages.InvalidTooManySubscriptions);
        }

        [Fact]
        public async Task UnknownSubscriptionTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            await ws.SendNegentropyMessageAsync("abcd", msg);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("abcd");
            received[2].GetString().Should().Be(Messages.Negentropy.InvalidUnknownId);
        }

        [Fact]
        public async Task ClosingSubscriptionTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            // open
            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [1] }, msg);
            await ws.ReceiveOnceAsync();
            
            // close
            await ws.SendNegentropyCloseAsync("abcd");

            // msg
            await ws.SendNegentropyMessageAsync("abcd", msg);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString().Should().Be("NEG-ERR");
            received[1].GetString().Should().Be("abcd");
            received[2].GetString().Should().Be(Messages.Negentropy.InvalidUnknownId);
        }

        [Fact]
        public async Task LargeSetSyncTest()
        {
            using var db = this.factory.Services.GetRequiredService<IDbContextFactory<NetstrDbContext>>().CreateDbContext();

            // seed
            var now = DateTimeOffset.UtcNow;
            var events = Enumerable
                .Range(0, 400)
                .Select(x => new EventEntity 
                { 
                    EventContent = "", 
                    EventCreatedAt = now.AddSeconds(x),
                    EventId = Convert.ToHexString(SHA256.HashData(BitConverter.GetBytes(x))).ToLower(),
                    EventKind = x < 300 ? 1 : 999, 
                    EventPublicKey = "",
                    EventSignature = "",
                    FirstSeen = now,
                    Tags = []
                })
                .ToArray();

            db.Events.AddRange(events);
            db.SaveChanges();
            
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var neg = new NegentropyBuilder(new NegentropyOptions()).Build();
            var msg = neg.Initiate();

            // open
            await ws.SendNegentropyOpenAsync("abcd", new Messaging.Models.SubscriptionFilterRequest { Kinds = [1] }, msg);
            var received = await ws.ReceiveOnceAsync();

            int i = 10;
            var needIds = new List<string>();

            while (i-- > 0)
            {
                received[0].GetString().Should().Be("NEG-MSG");
                received[1].GetString().Should().Be("abcd");

                var r = neg.Reconcile(received[2].GetString());

                if (r.NeedIds.Any())
                {
                    needIds.AddRange(r.NeedIds);
                }

                if (string.IsNullOrEmpty(r.Query))
                {
                    break;
                }

                await ws.SendNegentropyMessageAsync("abcd", r.Query);
                received = await ws.ReceiveOnceAsync();
            }

            needIds.Should().HaveCount(300);
            i.Should().BeLessThan(9);
        }
    }
}
