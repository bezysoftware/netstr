using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Handlers
{
    /// <summary>
    /// Regular events are stored by the relay. Duplicates are ignored.
    /// </summary>
    public class RegularEventHandler : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public RegularEventHandler(
            ILogger<RegularEventHandler> logger,
            IOptions<AuthOptions> auth,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, auth, adapters)
        {
            this.db = db;
        }

        // this event handler also serves as a fallback for all unknown events
        public override bool CanHandleEvent(Event e) => true;

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();

            if (await db.Events.IsDeleted(e.Id))
            {
                this.logger.LogInformation($"Event {e.Id} was already deleted");
                await sender.SendNotOkAsync(e.Id, Messages.InvalidDeletedEvent);
                return;
            }

            db.Add(e.ToEntity(DateTimeOffset.UtcNow));

            // save 
            await db.SaveChangesAsync();

            // reply
            await sender.SendOkAsync(e.Id);

            // broadcast
            await BroadcastEventAsync(e);
        }
    }
}
