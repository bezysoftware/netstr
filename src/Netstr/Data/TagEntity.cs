namespace Netstr.Data
{
    public class TagEntity
    {
        public required string Name { get; set; }

        public required string[] Values { get; set; }

        public EventEntity? Event { get; set; }
        
        public int EventId { get; set; }
    }
}
