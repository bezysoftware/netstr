using FluentAssertions;
using Netstr.Messaging.Models;
using Netstr.Options;
using Netstr.Tests.NIPs;
using Npgsql.Internal;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text.Json;

namespace Netstr.Tests
{
    public class LimitsTests : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory factory;

        public LimitsTests(WebApplicationFactory factory)
        {
            factory.Limits = new LimitsOptions
            {
                MinPowDifficulty = 0, // covered by a NIP-13 test
                MaxCreatedAtLowerOffset = 10,
                MaxCreatedAtUpperOffset = 10,
                MaxEventTags = 2,
                MaxInitialLimit = 5,
                MaxFilters = 2,
                MaxSubscriptionIdLength = 5,
                MaxSubscriptions = 1
            };
            this.factory = factory;
        }

        [Theory]
        [InlineData("Hello", "EOSE")]
        [InlineData("Too long", "CLOSED")]
        public async Task SubscriptionIdTests(string id, string expected)
        {
            using WebSocket ws = await ConnectWebSocketAsync();

            await ws.SendReqAsync(id, [new () { Kinds = [1] }]);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(2, "EOSE")]
        [InlineData(3, "CLOSED")]
        public async Task SubscriptionFiltersTests(int filters, string expected)
        {
            using WebSocket ws = await ConnectWebSocketAsync();

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
            using WebSocket ws = await ConnectWebSocketAsync();

            await ws.SendReqAsync("id", [new() { Limit = limit }]);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task SubscriptionCountTest()
        {
            using WebSocket ws = await ConnectWebSocketAsync();

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
            using WebSocket ws = await ConnectWebSocketAsync();

            var e = new Event
            {
                Id = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(offset),
                Kind = 10000,
                PublicKey = "5758137ec7f38f3d6c3ef103e28cd9312652285dab3497fe5e5f6c5c0ef45e75",
                Tags = [],
                Signature = ""
            };

            var obj = (object[])[
                0,
                e.PublicKey,
                e.CreatedAt.ToUnixTimeSeconds(),
                e.Kind,
                e.Tags,
                e.Content
            ];

            var id = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(obj))).ToLower();

            e = e with
            {
                Id = id,
                Signature = Helpers.Sign(id, "512a14752ed58380496920da432f1c0cdad952cd4afda3d9bfa51c2051f91b02")
            };

            // first sub succeeds
            await ws.SendEventAsync(e);
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().BeEquivalentTo("OK");
            received[1].GetString()?.Should().BeEquivalentTo(id);
            received[2].GetBoolean().Should().Be(expected);
        }

        private async Task<WebSocket> ConnectWebSocketAsync()
        {
            return await this.factory.Server.CreateWebSocketClient().ConnectAsync(new Uri($"ws://localhost"), CancellationToken.None);
        }
    }
}
