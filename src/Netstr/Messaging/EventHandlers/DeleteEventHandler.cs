using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.EventHandlers
{
    /// <summary>
    /// Delete events are special type of regular event which mark other events as deleted.
    /// </summary>
    public class DeleteEventHandler : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public DeleteEventHandler(
            ILogger<DeleteEventHandler> logger,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db) 
            : base(logger, adapters)
        {
            this.db = db;
        }

        public override bool CanHandleEvent(Event e) => e.IsDelete();

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();
            using var tx = await db.Database.BeginTransactionAsync();

            var now = DateTimeOffset.UtcNow;

            // delete by EventId
            var deleteIds = GetEventIdsToDelete(db, e.Tags);
            await db.Events
                .Where(x => x.EventPublicKey == e.PublicKey) // only delete own events
                .Where(x => x.EventKind != EventKind.Delete) // cannnot delete a delete event
                .Where(x => !x.DeletedAt.HasValue)           // not deleted yet
                .Where(x => deleteIds.Contains(x.EventId))   // in the list of ids
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

        private IEnumerable<string> GetEventIdsToDelete(NetstrDbContext db, string[][] tags)
        {
            var regularEvents = tags
                .Where(x => x.Length >= 2 && x[0] == EventTag.Event)
                .Select(x => x[1])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();

            return regularEvents;
        }
    }
}
