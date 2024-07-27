using Netstr.Messaging.Matching;
using Netstr.Messaging.Models;
using System.Runtime.CompilerServices;

namespace Netstr.Tests.Events
{
    /// <summary>
    /// Tests taken mostly from nostream: https://github.com/Cameri/nostream
    /// </summary>
    public class FilterEventMatchingTests
    {
        private Event e = new()
        {
            Id = "6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407",
            PublicKey = "22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793",
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1648351380),
            Kind = 1,
            Tags = [
              [
                "p",
                "8355095016fddbe31fcf1453b26f613553e9758cf2263e190eac8fd96a3d3de9",
                "wss://nostr-pub.wellorder.net",
              ],
              [
                "e",
                "7377fa81fc6c7ae7f7f4ef8938d4a603f7bf98183b35ab128235cc92d4bebf96",
                "wss://nostr-relay.untethr.me",
              ],
            ],
            Content = "I\"ve set up mirroring between relays = https://i.imgur.com/HxCDipB.png",
            Signature = "b37adfed0e6398546d623536f9ddc92b95b7dc71927e1123266332659253ecd0ffa91ddf2c0a82a8426c5b363139d28534d6cac893b8a810149557a3f6d36768",
        };

        [Fact]
        public void TrueForEmptyFilter()
        {
            var result = SubscriptionFilterMatcher.IsMatch(new SubscriptionFilter(), this.e);

            Assert.True(result);
        }

        [Theory]
        [InlineData("6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407", true)]
        [InlineData("different", false)]
        [InlineData("6b3cdd0302ded8068a", false)]
        public void IdsFilterTests(string id, bool expectation)
        {
            var filter = new SubscriptionFilter([id], [], [], null, null, 0, []);
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData("22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793", true)]
        [InlineData("different", false)]
        [InlineData("22e804d26ed16b68db52", false)]
        public void AuthorsFilterTests(string author, bool expectation)
        {
            var filter = new SubscriptionFilter([], [author], [], null, null, 0, []);
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expectation, result);
        }

        [Theory]
        [InlineData(-5, false)]
        [InlineData(1, true)]
        public void KindsFilterTests(int kind, bool expecation)
        {
            var filter = new SubscriptionFilter([], [], [kind], null, null, 0, []);
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expecation, result);
        }

        [Theory]
        [InlineData(1648351379, true)]
        [InlineData(1648351380, true)]
        [InlineData(1648351381, false)]
        public void SinceFilterTests(int since, bool expecation)
        {
            var filter = new SubscriptionFilter([], [], [], DateTimeOffset.FromUnixTimeSeconds(since), null, 0, []);
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expecation, result);
        }

        [Theory]
        [InlineData(1648351379, false)]
        [InlineData(1648351380, true)]
        [InlineData(1648351381, true)]
        public void UntilFilterTests(int until, bool expecation)
        {
            var filter = new SubscriptionFilter([], [], [], null, DateTimeOffset.FromUnixTimeSeconds(until), 0, []);
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expecation, result);
        }

        [Theory]
        [InlineData("6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407", "abcd", 1, 1648351379, 1648351381, false)]
        [InlineData("abcd", "22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793", 1, 1648351379, 1648351381, false)]
        [InlineData("6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407", "22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793", 1, 1648351379, 1648351379, false)]
        [InlineData("6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407", "22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793", 2, 1648351379, 1648351381, false)]
        [InlineData("abcd,6b3cdd0302ded8068ad3f0269c74423ca4fee460f800f3d90103b63f14400407", "abcd,22e804d26ed16b68db5259e78449e96dab5d464c8f470bda3eb1a70467f2c793", 1, 1648351379, 1648351381, true)]
        public void MultipleFiltersTest(string ids, string authors, int kind, int since, int until, bool expecation)
        {
            var filter = new SubscriptionFilter(
                ids.Split(","),
                authors.Split(","),
                [kind],
                DateTimeOffset.FromUnixTimeSeconds(since),
                DateTimeOffset.FromUnixTimeSeconds(until),
                0,
                []);

            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.Equal(expecation, result);
        }

        [Fact]
        public void SingleTagsMatchTest()
        {
            var filter = new SubscriptionFilter(
                [],
                [],
                [],
                null, null, 0,
                new()
                {
                    ["e"] = ["7377fa81fc6c7ae7f7f4ef8938d4a603f7bf98183b35ab128235cc92d4bebf96"]
                });
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.True(result);
        }

        [Fact]
        public void MultipleTagsMatchTest()
        {
            var filter = new SubscriptionFilter(
                [],
                [],
                [],
                null, null, 0,
                new()
                {
                    ["e"] = ["7377fa81fc6c7ae7f7f4ef8938d4a603f7bf98183b35ab128235cc92d4bebf96"],
                    ["p"] = ["abcd", "8355095016fddbe31fcf1453b26f613553e9758cf2263e190eac8fd96a3d3de9"]
                });
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.True(result);
        }

        [Fact]
        public void SomeTagsDoNotMatchTest()
        {
            var filter = new SubscriptionFilter(
                [],
                [],
                [],
                null, null, 0,
                new()
                {
                    ["e"] = ["abcd"],
                    ["p"] = ["8355095016fddbe31fcf1453b26f613553e9758cf2263e190eac8fd96a3d3de9"]
                });
            var result = SubscriptionFilterMatcher.IsMatch(filter, this.e);

            Assert.False(result);
        }
    }
}
