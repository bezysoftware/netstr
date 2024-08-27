using Netstr.Messaging.Models;

namespace Netstr.Messaging.Subscriptions.Validators
{
    public interface ISubscriptionRequestValidator
    {
        /// <summary>
        /// Verifies whether client can subscribe with given id and filters.
        /// </summary>
        string? CanSubscribe(string id, ClientContext context, IEnumerable<SubscriptionFilter> filters);
    }
}
