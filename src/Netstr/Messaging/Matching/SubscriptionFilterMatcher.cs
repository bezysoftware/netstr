using Netstr.Extensions;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.Matching
{
    public static class SubscriptionFilterMatcher
    {
        /// <summary>
        /// Returns whether the given event <paramref name="e"/> satisfies conditions in <paramref name="filter"/>
        /// </summary>
        public static bool IsMatch(SubscriptionFilter filter, Event e)
        {
            Func<bool>[] filters = [
                () => filter.Ids.EmptyOrAny(x => x == e.Id),
                () => filter.Authors.EmptyOrAny(x => x == e.PublicKey),
                () => filter.Kinds.EmptyOrAny(x => x == e.Kind),
                () => !filter.Since.HasValue || filter.Since <= e.CreatedAt,
                () => !filter.Until.HasValue || filter.Until >= e.CreatedAt,
                () => filter.Tags.All(tag => e.Tags.Any(x => tag.Key == x[0] && tag.Value.Contains(x[1])))
            ];

            return filters.All(x => x());
        }
    }
}
