using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Extensions;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Handlers
{
    public class VanishEventHandler : EventHandlerBase
    {
        private readonly IDbContextFactory<NetstrDbContext> db;
        private readonly IUserCache userCache;
        private readonly IHttpContextAccessor http;

        private readonly static string AllRelaysValue = "ALL_RELAYS";

        public VanishEventHandler(
            ILogger<EventHandlerBase> logger, 
            IOptions<AuthOptions> auth, 
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db,
            IUserCache userCache,
            IHttpContextAccessor http)
            : base(logger, auth, adapters)
        {
            this.db = db;
            this.userCache = userCache;
            this.http = http;
        }

        public override bool CanHandleEvent(Event e) => e.IsRequestToVanish();

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            var ctx = this.http.HttpContext?.Request ?? throw new InvalidOperationException("HttpContext not set");
            var user = this.userCache.GetByPublicKey(e.PublicKey);

            var path = ctx.GetNormalizedUrl();
            var relays = e.GetNormalizedRelayValues();

            // check 'relay' tag matches current url or is set to ALL_RELAYS
            if (!relays.Any(x => x == path || x == AllRelaysValue))
            {
                throw new MessageProcessingException(e, string.Format(Messages.InvalidWrongTagValue, EventTag.Relay));
            }

            using var db = this.db.CreateDbContext();
            using var tx = db.Database.BeginTransaction();

            // delete all user's events (or tagged GiftWraps) from before the vanish event
            await db.Events
                .Include(x => x.Tags)
                .Where(x => 
                    (x.EventPublicKey == e.PublicKey || 
                    (x.EventKind == EventKind.GiftWrap && x.Tags.Any(t => t.Name == EventTag.PublicKey && t.Value == e.PublicKey))) && 
                    x.EventCreatedAt <= e.CreatedAt)
                .ExecuteDeleteAsync();

            // insert vanish entity to db
            db.Events.Add(e.ToEntity(DateTimeOffset.UtcNow));

            // save
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            // set vanished in cache
            this.userCache.Vanish(e.PublicKey, e.CreatedAt);

            // reply
            await sender.SendOkAsync(e.Id);

            // broadcast
            await BroadcastEventAsync(e);
        }
    }
}
