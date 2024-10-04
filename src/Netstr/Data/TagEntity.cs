namespace Netstr.Data
{
    public class TagEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string? Value { get; set; }
        
        public required string[] OtherValues { get; set; }

        public EventEntity? Event { get; set; }
        
        public int EventId { get; set; }
    }
}
