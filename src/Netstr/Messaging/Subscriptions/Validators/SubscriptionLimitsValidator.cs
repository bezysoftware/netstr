using Microsoft.Extensions.Options;
using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.MessageHandlers.Negentropy;
using Netstr.Messaging.Models;
using Netstr.Options;
using Netstr.Options.Limits;

namespace Netstr.Messaging.Subscriptions.Validators
{
    /// <summary>
    /// Checks given subscription request for configured limits.
    /// </summary>
    public class SubscriptionLimitsValidator : ISubscriptionRequestValidator
    {
        protected readonly IOptions<LimitsOptions> limits;

        public SubscriptionLimitsValidator(IOptions<LimitsOptions> limits)
        {
            this.limits = limits;
        }

        public string? CanSubscribe(string id, ClientContext context, IEnumerable<SubscriptionFilter> filters)
        {
            var limits = GetLimits();

            if (limits.MaxSubscriptionIdLength > 0 && id.Length > limits.MaxSubscriptionIdLength)
            {
                return Messages.InvalidSubscriptionIdTooLong;
            }
            else if (limits.MaxFilters > 0 && filters.Count() > limits.MaxFilters)
            {
                return Messages.InvalidTooManyFilters;
            }
            else if (limits.MaxInitialLimit > 0 && filters.Any(x => x.Limit > limits.MaxInitialLimit))
            {
                return Messages.InvalidLimitTooHigh;
            }

            return null;
        }

        public virtual bool IsApplicable(FilterMessageHandlerBase handler)
        {
            return handler is not NegentropyOpenHandler;
        }

        protected virtual SubscriptionLimits GetLimits()
        {
            return this.limits.Value.Subscriptions;
        }
    }
}
