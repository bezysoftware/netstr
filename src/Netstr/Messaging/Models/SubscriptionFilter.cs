using Netstr.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Netstr.Messaging.Models
{
    public record SubscriptionFilterRequest
    {
        [JsonPropertyName("ids")]
        public string[]? Ids { get; init; }

        [JsonPropertyName("authors")]
        public string[]? Authors { get; init; }

        [JsonPropertyName("kinds")]
        public long[]? Kinds { get; init; }

        [JsonPropertyName("since")]
        [JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTimeOffset? Since { get; init; }

        [JsonPropertyName("until")]
        [JsonConverter(typeof(UnixTimestampJsonConverter))]
        public DateTimeOffset? Until { get; init; }

        [JsonPropertyName("limit")]
        public int? Limit { get; init; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalData { get; set; }
    }

    public record SubscriptionFilter(
        string[] Ids,
        string[] Authors,
        long[] Kinds,
        DateTimeOffset? Since,
        DateTimeOffset? Until,
        int? Limit,
        Dictionary<string, string[]> OrTags,
        Dictionary<string, string[]> AndTags)
    {
        public SubscriptionFilter()
            : this([], [], [], null, null, null, [], [])
        {
        }
    }
}