using System.Text.Json;

namespace Netstr.Messaging
{
    public record MessageBatch 
    {
        public MessageBatch(IEnumerable<object[]> messages)
        {
            Messages = messages
                .Select(x => JsonSerializer.SerializeToUtf8Bytes(x))
                .ToArray();
        }

        public MessageBatch(string id, IEnumerable<object[]> messages)
            : this(messages)
        {
            Id = id;
        }

        public string? Id { get; }

        public IEnumerable<byte[]> Messages { get; set; }

        public bool IsCancelled { get; private set; }

        public void Cancel()
        {
            IsCancelled = true;
        }

        public static MessageBatch Single(object[] message) => new MessageBatch([message]);
    }
}
