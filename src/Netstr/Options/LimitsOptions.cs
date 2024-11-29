using Netstr.Options.Limits;

namespace Netstr.Options
{
    public class LimitsOptions
    {
        public LimitsOptions()
        {
            Subscriptions = new();
            Events = new();
            Negentropy = new();
        }

        public int MaxPayloadSize { get; init; }

        public required SubscriptionLimits Subscriptions { get; init; }
        
        public required EventLimits Events { get; init; }
        
        public required NegentropyLimits Negentropy { get; init; }
    }
}
