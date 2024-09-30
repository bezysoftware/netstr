using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Handlers
{
    /// <summary>
    /// Delete events are special type of regular event which mark other events as deleted.
    /// </summary>
    public class DeleteEventHandler : EventHandlerBase
    {
        private record ReplaceableEventRef(int Kind, string PublicKey, string? Deduplication) { }

        private readonly IDbContextFactory<NetstrDbContext> db;

        public DeleteEventHandler(
            ILogger<DeleteEventHandler> logger,
            IOptions<AuthOptions> auth,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, auth, adapters)
        {
            this.db = db;
        }

        public override bool CanHandleEvent(Event e) => e.IsDelete();

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();
            using var tx = await db.Database.BeginTransactionAsync();

            var now = DateTimeOffset.UtcNow;

            // delete events (= mark as deleted)
            var regularEventIds = GetRegularEventIds(e.Tags);
            var replaceableQuery = GetReplaceableQuery(db, e, now);

            await db.Events
                .Where(x => x.EventPublicKey == e.PublicKey) // only delete own events
                .Where(x => x.EventKind != EventKind.Delete) // cannnot delete a delete event
                .Where(x => !x.DeletedAt.HasValue)           // not deleted yet
                .Where(x => regularEventIds.Contains(x.EventId) || replaceableQuery.Contains(x.EventId))
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.DeletedAt, now));

            db.Add(e.ToEntity(now));

            // save 
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            // reply
            await sender.SendOkAsync(e.Id);

            // broadcast
            await BroadcastEventAsync(e);
        }

        private IEnumerable<string> GetRegularEventIds(string[][] tags)
        {
            return tags
                .Where(x => x.Length >= 2 && x[0] == EventTag.Event)
                .Select(x => x[1])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct();
        }

        private IQueryable<string> GetReplaceableQuery(NetstrDbContext db, Event e, DateTimeOffset now)
        {
            var replacableEvents = e.Tags
                .Where(x => x.Length >= 2 && x[0] == EventTag.ReplaceableEvent)
                .Select(x => ParseReplaceableTag(x[1]))
                .WhereNotNull()
                .ToArray();

            var replaceableQuery = db.Events.Where(x => false);

            foreach (var re in replacableEvents)
            {
                var query = db.Events.Where(x => x.EventKind == re.Kind);
                replaceableQuery = replaceableQuery.Union(query);
            }

            return replaceableQuery
                .Where(x => x.EventCreatedAt <= e.CreatedAt) // only delete those before the deletion request
                .Select(x => x.EventId);
        }

        private ReplaceableEventRef? ParseReplaceableTag(string tag)
        {
            var parsed = tag.Split(":", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parsed.Length < 2)
            {
                return null;
            }

            if (!int.TryParse(parsed[0], out var kind))
            {
                return null;
            }

            return new(kind, parsed[1], parsed.Length > 2 ? parsed[2] : null);
        }
    }
}
