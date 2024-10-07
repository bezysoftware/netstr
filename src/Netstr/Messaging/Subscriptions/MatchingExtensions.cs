using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.Subscriptions
{
    public static class MatchingExtensions
    {
        /// <summary>
        /// Returns whether the given event <paramref name="e"/> satisfies conditions in any of the given <paramref name="filters"/>
        /// </summary>
        public static bool IsAnyMatch(this IEnumerable<SubscriptionFilter> filters, Event e)
        {
            return filters.Any(x => SubscriptionFilterMatcher.IsMatch(x, e));
        }

        /// <summary>
        /// Filters database events based on supplied filters.
        /// </summary>
        public static IQueryable<EventEntity> WhereAnyFilterMatches(
            this DbSet<EventEntity> entities,
            IEnumerable<SubscriptionFilter> filters,
            IEnumerable<long> protectedKinds,
            string? authenticatedPublicKey,
            int maxLimit)
        {
            return filters
                .Select(filter => entities
                    .Include(x => x.Tags)
                    .Where(x =>
                        (filter.Authors.Contains(x.EventPublicKey) || !filter.Authors.Any()) &&
                        (filter.Ids.Contains(x.EventId) || !filter.Ids.Any()) &&
                        (filter.Kinds.Contains(x.EventKind) || !filter.Kinds.Any()) &&
                        (filter.Since <= x.EventCreatedAt || !filter.Since.HasValue) &&
                        (filter.Until >= x.EventCreatedAt || !filter.Until.HasValue))
                    .WhereOrTags(filter.OrTags)
                    .WhereAndTags(filter.AndTags)
                    .Where(x => !protectedKinds.Contains(x.EventKind) || x.EventPublicKey == authenticatedPublicKey || x.Tags.Any(tag => tag.Name == EventTag.PublicKey && tag.Value == authenticatedPublicKey))
                    .OrderByDescending(x => x.EventCreatedAt)
                    .ThenBy(x => x.EventId)
                    .Take(filter.Limit.HasValue && filter.Limit.Value < maxLimit ? filter.Limit.Value : maxLimit))
                .Aggregate((acc, x) => acc.Union(x))
                .AsNoTracking();
        }

        /// <summary>
        /// Filters database events based on supplied filters with no auth.
        /// </summary>
        public static IQueryable<EventEntity> WhereAnyFilterMatches(
            this DbSet<EventEntity> entities,
            IEnumerable<SubscriptionFilter> filters,
            int maxLimit)
        {
            return WhereAnyFilterMatches(entities, filters, [], null, maxLimit);
        }

        private static IQueryable<EventEntity> WhereOrTags(this IQueryable<EventEntity> entities, IDictionary<string, string[]> tags)
        {
            foreach (var tag in tags)
            {
                entities = entities.Where(e => e.Tags.Any(etag => etag.Name == tag.Key && tag.Value.Contains(etag.Value)));
            }

            return entities;
        }

        private static IQueryable<EventEntity> WhereAndTags(this IQueryable<EventEntity> entities, IDictionary<string, string[]> tags)
        {
            foreach (var tag in tags)
            {
                foreach (var tagValue in tag.Value)
                {
                    entities = entities.Where(e => e.Tags.Any(etag => etag.Name == tag.Key && etag.Value == tagValue));
                }
            }

            return entities;
        }
    }
}
