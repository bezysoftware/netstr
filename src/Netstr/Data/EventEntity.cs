namespace Netstr.Data
{
    public class EventEntity
    {
        public int Id { get; set; }

        public required string EventId { get; set; }
        
        public required string EventPublicKey { get; set; }

        public required DateTimeOffset EventCreatedAt { get; set; }
        
        public required long EventKind { get; set; }
        
        public required string EventContent { get; set; }
        
        public required string EventSignature { get; set; }
        
        public string? EventDeduplication { get; set; }

        public DateTimeOffset? EventExpiration { get; set; }

        public required DateTimeOffset FirstSeen { get; set; }

        public required ICollection<TagEntity> Tags { get; set; }
    }
}
