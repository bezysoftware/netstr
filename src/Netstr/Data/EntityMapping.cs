using Netstr.Messaging.Models;

namespace Netstr.Data
{
    public static class EntityMapping
    {
        public static EventEntity ToEntity(this Event e, DateTimeOffset firstSeen)
        {
            return new EventEntity
            {
                FirstSeen = firstSeen,
                EventContent = e.Content,
                EventCreatedAt = e.CreatedAt,
                EventId = e.Id,
                EventKind = e.Kind,
                EventPublicKey = e.PublicKey,
                EventSignature = e.Signature,
                EventDeduplication = e.IsAddressable() 
                    ? e.GetDeduplicationValue() 
                    : null,
                Tags = e.Tags.Select(x => new TagEntity
                {
                    Name = x.First(),
                    Values = x.Skip(1).ToArray()
                }).ToArray(),
            };
        }
    }
}
