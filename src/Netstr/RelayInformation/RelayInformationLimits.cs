using System.Text.Json.Serialization;

namespace Netstr.RelayInformation
{
    public record RelayInformationLimits
    {
        [JsonPropertyName("min_pow_difficulty")]
        public required int MinPowDifficulty { get; init; }
    }
}
