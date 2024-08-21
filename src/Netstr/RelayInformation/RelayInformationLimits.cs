using System.Text.Json.Serialization;

namespace Netstr.RelayInformation
{
    public record RelayInformationLimits
    {
        [JsonPropertyName("min_pow_difficulty")]
        public required int MinPowDifficulty { get; init; }

        [JsonPropertyName("max_limit")]
        public required int MaxLimit { get; init; }

        [JsonPropertyName("max_filters")]
        public required int MaxFilters { get; init; }

        [JsonPropertyName("max_subscriptions")]
        public required int MaxSubscriptions { get; init; }

        [JsonPropertyName("max_subid_length")]
        public required int MaxSubscriptionIdLength { get; init; }

        [JsonPropertyName("max_event_tags")]
        public required int MaxEventTags { get; init; }

        [JsonPropertyName("created_at_lower_limit")]
        public required int CreatedAtLowerLimit { get; init; }

        [JsonPropertyName("created_at_upper_limit")]
        public required int CreatedAtUpperLimit { get; init; }
    }
}
