using FluentAssertions;
using Netstr.Messaging;
using Netstr.Messaging.Models;
using Netstr.Options;
using Netstr.Tests.NIPs;
using System.Net.WebSockets;

namespace Netstr.Tests
{
    /// <summary>
    /// Basic scenarios are covered by NIP-42 specflow tests, these are netstr specific.
    /// </summary>
    public class AuthTests
    {
        private readonly WebApplicationFactory factory;

        public AuthTests()
        {
            this.factory = new WebApplicationFactory();
        }

        [Fact]
        public async Task PublishAuthModeTest()
        {
            using WebSocket ws = await ConnectWebSocketAsync(AuthMode.Publishing);

            var auth = await ws.ReceiveOnceAsync();

            var e = new Event
            {
                Id = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                Kind = 1,
                PublicKey = Alice.PublicKey,
                Tags = [],
                Signature = ""
            };

            e = Helpers.FinalizeEvent(e, Alice.PrivateKey);

            await ws.SendEventAsync(e);
            var ok = await ws.ReceiveOnceAsync();

            ok[2].GetBoolean().Should().BeFalse();
            ok[3].GetString().Should().Be(Messages.AuthRequiredPublishing);

            e = new Event
            {
                Id = "",
                Signature = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                PublicKey = Alice.PublicKey,
                Tags = [
                    ["relay", "ws://localhost"],
                    ["challenge", auth[1].ToString()]
                ],
                Kind = EventKind.Auth
            };

            e = Helpers.FinalizeEvent(e, Alice.PrivateKey);

            await ws.SendAuthAsync(e);
            ok = await ws.ReceiveOnceAsync();

            ok[2].GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task DisabledAuthModeDoesntSendAuth()
        {
            using WebSocket ws = await ConnectWebSocketAsync(AuthMode.Disabled);

            await ws.SendReqAsync("test", [new() { Kinds = [1] }]);
            
            var received = await ws.ReceiveOnceAsync();

            received[0].GetString()?.Should().NotBe("AUTH");
        }

        [Fact]
        public async Task WrongAuthEventKindTest()
        {
            using WebSocket ws = await ConnectWebSocketAsync(AuthMode.Publishing);

            var auth = await ws.ReceiveOnceAsync();

            var e = new Event
            {
                Id = "",
                Signature = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                PublicKey = Alice.PublicKey,
                Tags = [
                    ["relay", "ws://localhost"],
                    ["challenge", auth[1].ToString()]
                ],
                Kind = EventKind.Auth + 1
            };

            e = Helpers.FinalizeEvent(e, Alice.PrivateKey);

            await ws.SendAuthAsync(e);
            var ok = await ws.ReceiveOnceAsync();

            ok[2].GetBoolean().Should().BeFalse();
        }

        private async Task<WebSocket> ConnectWebSocketAsync(AuthMode authMode = AuthMode.WhenNeeded)
        {
            this.factory.AuthMode = authMode;
            return await this.factory.Server.CreateWebSocketClient().ConnectAsync(new Uri($"ws://localhost"), CancellationToken.None);
        }
    }
}
