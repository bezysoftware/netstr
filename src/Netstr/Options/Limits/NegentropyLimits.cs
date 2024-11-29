namespace Netstr.Options.Limits
{
    public class NegentropyLimits : SubscriptionLimits
    {
        public int StaleSubscriptionPeriodSeconds { get; init; }
        public int StaleSubscriptionLimitSeconds { get; init; }
        public int MaxSubscriptionAgeSeconds { get; init; }
        public uint FrameSizeLimit { get; init; }
    }
}
