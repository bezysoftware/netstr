using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.EventHandlers
{
    /// <summary>
    /// Regular events are stored by the relay. Duplicates are ignored.
    /// </summary>
    public class RegularEventHandler : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;

        public RegularEventHandler(
            ILogger<RegularEventHandler> logger,
            IWebSocketAdapterCollection adapters, 
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, adapters)
        {
            this.db = db;
        }

        public override bool CanHandleEvent(Event e) => e.IsRegular();

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            using var db = this.db.CreateDbContext();

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
