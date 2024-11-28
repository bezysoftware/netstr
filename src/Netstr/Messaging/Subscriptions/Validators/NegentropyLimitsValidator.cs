using Microsoft.Extensions.Options;
using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.MessageHandlers.Negentropy;
using Netstr.Options;

namespace Netstr.Messaging.Subscriptions.Validators
{
    public class NegentropyLimitsValidator : SubscriptionLimitsValidator
    {
        public NegentropyLimitsValidator(IOptions<NegentropyLimitsOptions> limits) : base(limits)
        {
        }

        public override bool IsApplicable(FilterMessageHandlerBase handler)
        {
            return handler is NegentropyOpenHandler;
        }
    }
}
