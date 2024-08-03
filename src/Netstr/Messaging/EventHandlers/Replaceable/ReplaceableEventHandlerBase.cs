using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Models;
using System.Linq.Expressions;

namespace Netstr.Messaging.EventHandlers.Replaceable
{
    /// <summary>
    /// Replaceable are unique not with their Id, but with a custom combination of other properties (e.g. pubkey+kind).
    /// </summary>
    public abstract class ReplaceableEventHandlerBase : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public ReplaceableEventHandlerBase(
            ILogger<ReplaceableEventHandlerBase> logger,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, adapters)
        {
            this.db = db;
        }

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();

            var newEntity = e.ToEntity(DateTimeOffset.UtcNow);
            var existing = await db.Events
                .AsNoTracking()
                .Where(GetUniqueEntityExpression(newEntity))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                if (newEntity.EventCreatedAt < existing.EventCreatedAt)
                {
                    this.logger.LogInformation($"Newer event {e.ToStringUnique()} already exists, ignoring");
                    await sender.SendOkAsync(e.Id, Messages.DuplicateReplaceableEvent);
                    return;
                }

                db.Remove(existing);

                // copy over original first seen
                newEntity.FirstSeen = existing.FirstSeen;
            }
            
            db.Add(newEntity);
            
            // save 
            await db.SaveChangesAsync();

            // reply
            await sender.SendOkAsync(e.Id);

            // broadcast
            await BroadcastEventAsync(e);
        }

        /// <summary>
        /// Expression which identifies a unique replacable entity
        /// </summary>
        protected abstract Expression<Func<EventEntity, bool>> GetUniqueEntityExpression(EventEntity newEntity);
    }
}
