using FluentAssertions;
using Netstr.Messaging.Models;

namespace Netstr.Tests.Events
{
    public class EventDeduplicationTests
    {
        private Event CreateEvent(string[][] tags)
        {
            return new Event
            {
                Id = "",
                PublicKey = "",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1719434163),
                Kind = 1,
                Tags = tags,
                Content = "",
                Signature = ""
            };
        }

        [Fact]
        public void EventDeduplicationValueNullTest()
        {
            var e = CreateEvent([]);
            var d = e.GetDeduplicationValue();

            d.Should().BeNull();
        }

        [Fact]
        public void EventDeduplicationValueTest()
        {
            var e = CreateEvent([
                [ "d", "test", "test2" ]
            ]);
            var d = e.GetDeduplicationValue();

            d.Should().Be("test");
        }

        [Fact]
        public void EventDeduplicationValueMultipleTest()
        {
            var e = CreateEvent([
                [ "e", "e" ],
                [ "d" ],
                [ "d", "test", "test2" ],
                [ "d", "second", "second2" ]
            ]);
            var d = e.GetDeduplicationValue();

            d.Should().Be("test");
        }
    }
}
