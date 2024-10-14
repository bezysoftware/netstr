using FluentAssertions;
using Microsoft.Extensions.Options;
using Netstr.Messaging;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Tests
{
    public class RateLimitingTests
    {
        private readonly WebApplicationFactory factory;

        public RateLimitingTests()
        {
            this.factory = new WebApplicationFactory();
            this.factory.Limits = new LimitsOptions
            {
                MaxPayloadSize = 524288,
                MaxEventsPerMinute = 5,
                MaxSubscriptionsPerMinute = 2
            };
        }

        [Fact]
        public async Task EventsRateLimitedTest()
        {
            using var ws = await this.factory.ConnectWebSocketAsync();

            var limits = this.factory.Services.GetRequiredService<IOptions<LimitsOptions>>();

            var e = new Event
            {
                Id = "904559949fe0a7dcc43166545c765b4af823a63ef9f8177484596972478b662c",
                PublicKey = "07d8fd2ea9040aadd608d3a523f0e150d9811afc826a896f8f5be2a1ed25187c",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1721741818),
                Kind = 1,
                Tags = [],
                Content = "Hello!",
                Signature = "33f42d22335842cd02372340feb6cd14fb5e438d49fe9f6bdecd5baa683b8dd8b4501da35026f4f29f03137f2766942d6795c491a83145b431ee0f3477039a5c"
            };

            var replies = new List<JsonElement[]>();
            var tooManyCount = limits.Value.MaxEventsPerMinute + 1;

            _ = ws.ReceiveAsync(replies.Add);
            
            for (var i = 0; i < tooManyCount; i++)
            {
                await ws.SendEventAsync(e);
            }

            await Task.Delay(1000);

            replies.Should().HaveCount(tooManyCount);
            replies.SkipLast(1).Select(x => x[2].GetBoolean()).Should().AllBeEquivalentTo(true);
            
            var last = replies.Last();
            last[2].GetBoolean().Should().BeFalse();
            last[3].GetString().Should().Be(Messages.RateLimited);
        }

        [Fact]
        public async Task SubscriptionsRateLimitedTest()
        {
            using var ws = await this.factory.ConnectWebSocketAsync();

            var limits = this.factory.Services.GetRequiredService<IOptions<LimitsOptions>>();

            var replies = new List<JsonElement[]>();
            var tooManyCount = limits.Value.MaxSubscriptionsPerMinute + 1;

            _ = ws.ReceiveAsync(replies.Add);

            for (var i = 0; i < tooManyCount; i++)
            {
                await ws.SendReqAsync("toomanytest", [ new SubscriptionFilterRequest { Ids = ["1"] }]);
            }

            await Task.Delay(1000);

            replies.Should().HaveCount(tooManyCount);
            replies.SkipLast(1).Select(x => x[0].GetString()).Should().AllBeEquivalentTo("EOSE");

            var last = replies.Last();
            last[0].GetString().Should().Be("CLOSED");
            last[1].GetString().Should().Be("toomanytest");
            last[2].GetString().Should().Be(Messages.RateLimited);
        }
    }
}
