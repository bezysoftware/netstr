using Microsoft.EntityFrameworkCore.Diagnostics;
using Netstr.Converters;
using System.Numerics;
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

        public bool IsAddressable() => Kind is >= 30000 and < 40000;

        public bool IsDelete() => Kind == EventKind.Delete;

        public bool IsProtected() => Tags.Any(x => x.Length >= 1 && x[0] == EventTag.Protected);

        public string ToStringUnique()
        {
            return IsAddressable()
                ? $"{Id} | {Kind} | {PublicKey} | {GetDeduplicationValue()}"
                : $"{Id} | {Kind} | {PublicKey}";
        }

        public int GetDifficulty()
        {
            var hash = Convert.FromHexString(Id);
            var result = 0;

            foreach (var b in hash)
            {
                // LeadingZeroCount works over int (32 bits) but "hash" is a byte[] (8 bits)
                var zeroes = BitOperations.LeadingZeroCount(b) - 24;
                result += Math.Max(0, zeroes);

                if (zeroes != 8)
                {
                    break;
                }
            }

            return result;
        }

        public string? GetDeduplicationValue()
        {
            return Tags.FirstOrDefault(x => x.Length > 1 && x.FirstOrDefault() == EventTag.Deduplication)?[1];
        }
    }
}
