namespace Netstr.Messaging.Negentropy
{
    public class NegentropyProcessingException : MessageProcessingException
    {
        public NegentropyProcessingException(string id, string message, string? logMessage = null)
            : base(["NEG-ERR", id, message], logMessage ?? $"Negentropy request '{id}' failed: {message}")
        {
        }
    }
}
