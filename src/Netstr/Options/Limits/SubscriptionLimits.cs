namespace Netstr.Options.Limits
{
    public class SubscriptionLimits
    {
        public int MaxInitialLimit { get; init; }
        public int MaxFilters { get; init; }
        public int MaxSubscriptions { get; init; }
        public int MaxSubscriptionIdLength { get; init; }
        public int MaxSubscriptionsPerMinute { get; init; }
    }
}
