using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Events;
using Netstr.Messaging.Models;

namespace Netstr.Tests
{
    public class CleanupTests
    {
        private readonly WebApplicationFactory factory;

        public CleanupTests()
        {
            this.factory = new WebApplicationFactory();
        }

        [Theory]
        [InlineData("1", 1, 1)]
        [InlineData("1-", 1, int.MaxValue)]
        [InlineData("-10", int.MinValue, 10)]
        [InlineData("3-10", 3, 10)]
        public void KindRangeTests(string range, int expectedMin, int expectedMax)
        {
            var result = KindRange.Parse(range);

            result.MinKind.Should().Be(expectedMin);
            result.MaxKind.Should().Be(expectedMax);
        }

        [Fact]
        public async Task CleanupTest()
        {
            using var db = this.factory.Services.GetRequiredService<IDbContextFactory<NetstrDbContext>>().CreateDbContext();
            
            // seed
            var now = DateTimeOffset.UtcNow;
            EventEntity[] events = [
                CreateEvent("a", 0, now),
                CreateEvent("b", 0, now, now.AddDays(-8)),       // deleted
                CreateEvent("c", 0, now, null, now.AddDays(-8)), // expired
                CreateEvent("d", 17, now),
                CreateEvent("e", 17, now.AddDays(-15)),          // reaction
                CreateEvent("f", 40000, now),
                CreateEvent("g", 40000, now.AddDays(-8))         // unknown
            ];

            db.Events.AddRange(events);
            db.SaveChanges();

            var service = this.factory.Services.GetRequiredService<ICleanupService>();
            
            await service.RunCleanupAsync();

            var remaining = await db.Events.Select(x => x.EventId).ToArrayAsync();

            remaining.Should().BeEquivalentTo(["a", "d", "f"]);
        }

        private EventEntity CreateEvent(string id, int kind, DateTimeOffset created, DateTimeOffset? deleted = null, DateTimeOffset? expired = null)
        {
            return new EventEntity
            {
                EventContent = "",
                EventCreatedAt = created,
                EventId = id,
                EventKind = kind,
                EventPublicKey = "",
                EventSignature = "",
                DeletedAt = deleted,
                EventExpiration = expired,
                FirstSeen = created,
                Tags = []
            };
        }
    }
}
