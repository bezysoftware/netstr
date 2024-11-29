namespace Netstr.Messaging.Subscriptions
{
    public class SubscriptionProcessingException : MessageProcessingException
    {
        public SubscriptionProcessingException(string id, string message, string? logMessage = null)
            : base(["CLOSED", id, message], logMessage ?? $"Subscription request '{id}' failed: {message}")
        {
        }
    }
}
