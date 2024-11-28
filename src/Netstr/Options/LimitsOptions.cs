namespace Netstr.Options
{
    public class LimitsOptions
    {
        public int MaxPayloadSize { get; init; }
        public int MaxInitialLimit { get; init; }
        public int MinPowDifficulty { get; init; }
        public int MaxFilters { get; init; }
        public int MaxSubscriptions { get; init; }
        public int MaxSubscriptionIdLength { get; init; }
        public int MaxEventTags { get; init; }
        public int MaxCreatedAtLowerOffset { get; init; }
        public int MaxCreatedAtUpperOffset { get; init; }
        public int MaxPendingEvents { get; init; }
        public int MaxEventsPerMinute { get; init; }
        public int MaxSubscriptionsPerMinute { get; init; }
    }

    public class NegentropyLimitsOptions : LimitsOptions 
    {
        public int StaleSubscriptionPeriodSeconds { get; set; }
        public int StaleSubscriptionLimitSeconds { get; set; }
        public int MaxSubscriptionAgeSeconds { get; set; }
    }
}
