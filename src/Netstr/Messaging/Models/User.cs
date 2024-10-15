namespace Netstr.Messaging.Models
{
    public record User
    {
        public required string PublicKey { get; init; }

        public string? EventId { get; init; }

        public DateTimeOffset? LastVanished { get; init; }
    }
}
