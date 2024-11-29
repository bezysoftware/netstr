using Netstr.Messaging.MessageHandlers;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.Subscriptions.Validators
{
    public interface ISubscriptionRequestValidator
    {
        /// <summary>
        /// Returns whether this request validator is applicable for given message handler
        /// </summary>
        bool IsApplicable(FilterMessageHandlerBase handler);

        /// <summary>
        /// Verifies whether client can subscribe with given id and filters.
        /// </summary>
        string? CanSubscribe(string id, ClientContext context, IEnumerable<SubscriptionFilter> filters);
    }
}
