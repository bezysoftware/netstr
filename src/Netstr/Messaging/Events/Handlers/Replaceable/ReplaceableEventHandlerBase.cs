using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Events.Handlers;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Linq.Expressions;

namespace Netstr.Messaging.Events.Handlers.Replaceable
{
    /// <summary>
    /// Replaceable are unique not with their Id, but with a custom combination of other properties (e.g. pubkey+kind).
    /// </summary>
    public abstract class ReplaceableEventHandlerBase : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public ReplaceableEventHandlerBase(
            ILogger<ReplaceableEventHandlerBase> logger,
            IOptions<AuthOptions> auth,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, auth, adapters)
        {
            this.db = db;
        }

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();

            if (await db.Events.IsDeleted(e.Id))
            {
                this.logger.LogInformation($"Event {e.Id} was already deleted");
                await sender.SendNotOkAsync(e.Id, Messages.InvalidDeletedEvent);
                return;
            }

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
                    await sender.SendNotOkAsync(e.Id, Messages.DuplicateReplaceableEvent);
                    return;
                }

                // if event was previously deleted only accept newer events if they are newer than the deletion
                if (existing.DeletedAt.HasValue && newEntity.EventCreatedAt < existing.DeletedAt)
                {
                    this.logger.LogInformation($"Event {e.ToStringUnique()} was previously deleted");
                    await sender.SendNotOkAsync(e.Id, Messages.InvalidDeletedEvent);
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
