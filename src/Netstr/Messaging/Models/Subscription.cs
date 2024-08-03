namespace Netstr.Messaging.Models
{
    public record Subscription(SubscriptionFilter[] Filters, DateTimeOffset InitialLastSeenEventTime)
    {
    }
}
