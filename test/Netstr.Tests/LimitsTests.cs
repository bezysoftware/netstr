using FluentAssertions;
using Netstr.Messaging.Models;
using Netstr.Options;
using Netstr.Tests.NIPs;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;

namespace Netstr.Tests
{
    public class LimitsTests
    {
        private readonly WebApplicationFactory factory;

        public LimitsTests()
        {
            this.factory = new WebApplicationFactory();
            this.factory.MaxPayloadSize = 1024;
            this.factory.EventLimits = new Options.Limits.EventLimits
            {
                MinPowDifficulty = 0, // covered by a NIP-13 test
                MaxCreatedAtLowerOffset = 10,
                MaxCreatedAtUpperOffset = 10,
                MaxEventTags = 2,
            };
            this.factory.SubscriptionLimits = new Options.Limits.SubscriptionLimits
            {
                MaxInitialLimit = 5,
                MaxFilters = 2,
                MaxSubscriptionIdLength = 5,
                MaxSubscriptions = 1
            };
        }

        [Theory]
        [InlineData("Hello", "EOSE")]
        [InlineData("Too long", "CLOSED")]
        public async Task SubscriptionIdTests(string id, string expected)
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            await ws.SendReqAsync(id, [new () { Kinds = [1] }]);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(2, "EOSE")]
        [InlineData(3, "CLOSED")]
        public async Task SubscriptionFiltersTests(int filters, string expected)
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var requestFilters = Enumerable
                .Range(0, filters)
                .Select(x => new SubscriptionFilterRequest() { Kinds = [1] })
                .ToArray();
            
            await ws.SendReqAsync("id", requestFilters);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(5, "EOSE")]
        [InlineData(6, "CLOSED")]
        public async Task SubscriptionMaxLimitTests(int limit, string expected)
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            await ws.SendReqAsync("id", [new() { Limit = limit }]);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task SubscriptionCountTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            // first sub succeeds
            await ws.SendReqAsync("id", [new() { Limit = 1 }]);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo("EOSE");

            // same id replaces existing sub
            await ws.SendReqAsync("id", [new() { Limit = 1 }]);
            var received2 = await ws.ReceiveOnceAsync();

            received2[0].GetString()?.Should().BeEquivalentTo("EOSE");

            // second sub fails
            await ws.SendReqAsync("id2", [new() { Limit = 1 }]);
            var received3 = await ws.ReceiveOnceAsync();

            received3[0].GetString()?.Should().BeEquivalentTo("CLOSED");
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(20, false)]
        [InlineData(-20, false)]
        public async Task EventCreatedAtTest(int offset, bool expected)
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var e = new Event
            {
                Id = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(offset),
                Kind = 10000,
                PublicKey = Alice.PublicKey,
                Tags = [],
                Signature = ""
            };

            e = Helpers.FinalizeEvent(e, Alice.PrivateKey);

            // first sub succeeds
            await ws.SendEventAsync(e);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo("OK");
            received[1].GetString()?.Should().BeEquivalentTo(e.Id);
            received[2].GetBoolean().Should().Be(expected);
        }

        [Fact]
        public async Task PayloadTooLargeTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var payload = new byte[1025];

            await ws.SendAsync([payload]);
            await ws.ReceiveOnceAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));

            await ws.ReceiveAsync(Memory<byte>.Empty, CancellationToken.None);

            ws.State.Should().BeOneOf(WebSocketState.Closed, WebSocketState.CloseReceived);
            ws.CloseStatus.Should().Be(WebSocketCloseStatus.MessageTooBig);
        }

        [Fact]
        public async Task TooManyTagsTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var e = new Event
            {
                Id = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = 1,
                PublicKey = Alice.PublicKey,
                Tags = [["a"],["b"],["c"]],
                Signature = ""
            };

            e = Helpers.FinalizeEvent(e, Alice.PrivateKey);

            await ws.SendEventAsync(e);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo("OK");
            received[1].GetString()?.Should().BeEquivalentTo(e.Id);
            received[2].GetBoolean().Should().Be(false);
        }
    }
}
