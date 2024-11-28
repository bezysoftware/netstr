namespace Netstr.Options.Limits
{
    public class EventLimits
    {
        public int MinPowDifficulty { get; init; }
        public int MaxEventTags { get; init; }
        public int MaxCreatedAtLowerOffset { get; init; }
        public int MaxCreatedAtUpperOffset { get; init; }
        public int MaxPendingEvents { get; init; }
        public int MaxEventsPerMinute { get; init; }
    }
}
