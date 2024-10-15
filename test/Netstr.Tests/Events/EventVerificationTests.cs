using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Netstr.Extensions;
using Netstr.Messaging;
using Netstr.Messaging.Events;
using Netstr.Messaging.Events.Validators;
using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Text.Json;

namespace Netstr.Tests.Events
{
    public class EventVerificationTests
    {
        private readonly IEnumerable<IEventValidator> validators;

        public EventVerificationTests()
        {
            this.validators = new ServiceCollection()
                .AddOptions<LimitsOptions>().Services
                .AddLogging()
                .AddEventValidators()
                .AddSingleton<IUserCache, UserCache>()
                .BuildServiceProvider()
                .GetRequiredService<IEnumerable<IEventValidator>>();
        }

        [Fact]
        public void AcceptsValidEvent()
        {
            var e = new Event
            {
                Id = "fc01cf4f48a060b3f5fb4a60f7cbf53b1456aee1c2685d02dbf3592ae8c1143e",
                PublicKey = "56b926b41562f5562509fb052c57c1570e9d189f6a347f19043b9b46f6d24ccd",
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1719434163),
                Kind = 1,
                Tags = [],
                Content = "Hello world",
                Signature = "44224ca5edd01161f617a7347d4f0b1c9a8ccf7bfb3f70bd74db3d6e26f44aa5318f3d39c93f5769d24fa5e56bd98eed7cd23a114cc3412650678a0280ed94f4"
            };

            this.validators.ToList().ForEach(x => x.Validate(e, new ClientContext("test", "ip")).Should().BeNull());
        }

        [Theory]
        [InlineData(
            "",
            "56b926b41562f5562509fb052c57c1570e9d189f6a347f19043b9b46f6d24ccd",
            "44224ca5edd01161f617a7347d4f0b1c9a8ccf7bfb3f70bd74db3d6e26f44aa5318f3d39c93f5769d24fa5e56bd98eed7cd23a114cc3412650678a0280ed94f4",
            "Content changed",
            Messages.InvalidId)]
        [InlineData(
            "fc01cf4f48a060b3f5fb4a60f7cbf53b1456aee1c2685d02dbf3592ae8c1143e",
            "56b926b41562f5562509fb052c57c1570e9d189f6a347f19043b9b46f6d24ccd",
            "44224ca5edd01161f617a7347d4f0b1c9a8ccf7bfb3f70bd74db3d6e26f44aa5318f3d39c93f5769d24fa5e56bd98eed7cd23a114cc3412650678a0280ed94f4",
            "Content changed",
            Messages.InvalidId)]
        [InlineData(
            "fc01cf4f48a060b3f5fb4a60f7cbf53b1456aee1c2685d02dbf3592ae8c1143e",
            "56b926b41562f5562509fb052c57c1570e9d189f6a347f19043b9b46f6d24ccd",
            "Not a hex signature",
            "Hello world",
            Messages.InvalidSignature)]
        [InlineData(
            "fc01cf4f48a060b3f5fb4a60f7cbf53b1456aee1c2685d02dbf3592ae8c1143e",
            "56b926b41562f5562509fb052c57c1570e9d189f6a347f19043b9b46f6d24ccd",
            "54224ca5edd01161f617a7347d4f0b1c9a8ccf7bfb3f70bd74db3d6e26f44aa5318f3d39c93f5769d24fa5e56bd98eed7cd23a114cc3412650678a0280ed94f4",
            "Hello world",
            Messages.InvalidSignature)]
        public void RejectsIfValidationFails(string id, string pubkey, string signature, string content, string error)
        {
            var e = new Event
            {
                Id = id,
                PublicKey = pubkey,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(1719434163),
                Kind = 1,
                Tags = [],
                Content = content,
                Signature = signature
            };

            var result = this.validators.Select(x => x.Validate(e, new ClientContext("test", "ip"))).FirstOrDefault(x => x != null);

            result.Should().Be(error);
        }

        [Theory]
        // Empty event
        [InlineData("[ \"EVENT\" ]")]
        // Missing 'content'
        [InlineData("[ \"EVENT\", { \"id\": \"1\", \"pubkey\": \"\", \"created_at\": 0, \"kind\": 0, \"tags\": [], \"sig\": \"\" } ]")]
        // Extra 'foo'
        [InlineData("[ \"EVENT\", { \"foo\": \"1\", \"id\": \"\", \"pubkey\": \"\", \"created_at\": 0, \"kind\": 0, \"tags\": [], \"sig\": \"\", \"content\": \"\" } ]")]
        // Extra item in array
        [InlineData("[ \"EVENT\", { \"id\": \"1\", \"pubkey\": \"\", \"created_at\": 0, \"kind\": 0, \"tags\": [], \"sig\": \"\", \"content\": \"\" }, \"foo\" ]")]
        public void InvalidEventTest(string msg)
        {
            var docs = JsonSerializer.Deserialize<JsonDocument[]>(msg) ?? throw new NullReferenceException();

            var e = EventParser.TryParse(docs, out var ex);

            e.Should().BeNull();
        }
    }
}
