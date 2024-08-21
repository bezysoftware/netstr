namespace Netstr.Options
{
    public class LimitsOptions
    {
        public required int MaxInitialLimit { get; init; }
        public required int MinPowDifficulty { get; init; }
        public required int MaxFilters { get; init; }
        public required int MaxSubscriptions { get; init; }
        public required int MaxSubscriptionIdLength { get; init; }
        public required int MaxEventTags { get; init; }
        public required int MaxCreatedAtLowerOffset { get; init; }
        public required int MaxCreatedAtUpperOffset { get; init; }
    }
}
