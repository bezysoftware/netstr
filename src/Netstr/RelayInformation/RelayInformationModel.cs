using System.Text.Json.Serialization;

namespace Netstr.RelayInformation
{
    public record RelayInformationModel
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("description")]
        public required string Description { get; init; }

        [JsonPropertyName("contact")]
        public string? Contact { get; init; }
        
        [JsonPropertyName("pubkey")]
        public string? PublicKey { get; init; }
        
        [JsonPropertyName("supported_nips")]
        public required int[] SupportedNips { get; init; }

        [JsonPropertyName("version")]
        public string? SoftwareVersion { get; init; }

        [JsonPropertyName("software")]
        public string? Software { get; init; }
    }
}
