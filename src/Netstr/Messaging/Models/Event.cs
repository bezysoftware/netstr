using Netstr.Converters;
using System.Text.Json.Serialization;

namespace Netstr.Messaging.Models
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public record Event
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("pubkey")]
        public required string PublicKey { get; init; }
        
        [JsonPropertyName("kind")]
        public required long Kind { get; init; }
        
        [JsonPropertyName("tags")]
        public required string[][] Tags { get; init; }
        
        [JsonPropertyName("content")]
        public required string Content { get; init; }
        
        [JsonPropertyName("sig")]
        public required string Signature { get; init; }

        [JsonPropertyName("created_at")]
        [JsonConverter(typeof(UnixTimestampJsonConverter))]
        public required DateTimeOffset CreatedAt { get; init; }

        public bool IsRegular() => Kind is > 0 and < 10000 and not 3;

        public bool IsReplaceable() => Kind is >= 10000 and < 20000 or 0 or 3;

        public bool IsEphemeral() => Kind is >= 20000 and < 30000;

        public bool IsParametrizedReplaceable() => Kind is >= 30000 and < 40000;

        public bool IsDelete() => Kind == EventKind.Delete;

        public string ToStringUnique()
        {
            return IsParametrizedReplaceable()
                ? $"{Id} | {Kind} | {PublicKey} | {GetDeduplicationValue()}"
                : $"{Id} | {Kind} | {PublicKey}";
        }

        public string? GetDeduplicationValue()
        {
            return Tags.FirstOrDefault(x => x.Length > 1 && x.FirstOrDefault() == EventTag.Deduplication)?[1];
        }
    }
}
