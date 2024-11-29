using Microsoft.Extensions.Options;
using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.MessageHandlers.Negentropy;
using Netstr.Options;
using Netstr.Options.Limits;

namespace Netstr.Messaging.Subscriptions.Validators
{
    public class NegentropyLimitsValidator : SubscriptionLimitsValidator
    {
        public NegentropyLimitsValidator(IOptions<LimitsOptions> limits) : base(limits)
        {
        }

        public override bool IsApplicable(FilterMessageHandlerBase handler)
        {
            return handler is NegentropyOpenHandler;
        }

        protected override SubscriptionLimits GetLimits()
        {
            return this.limits.Value.Negentropy;
        }
    }
}
