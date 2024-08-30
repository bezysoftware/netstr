using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Linq.Expressions;

namespace Netstr.Messaging.Events.Handlers.Replaceable
{
    /// <summary>
    /// Replaceable events have a unique combination of pubkey+kind.
    /// </summary>
    public class ReplaceableEventHandler : ReplaceableEventHandlerBase
    {
        public ReplaceableEventHandler(
            ILogger<ReplaceableEventHandler> logger,
            IOptions<AuthOptions> auth,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, auth, adapters, db)
        {
        }

        public override bool CanHandleEvent(Event e) => e.IsReplaceable();

        protected override Expression<Func<EventEntity, bool>> GetUniqueEntityExpression(EventEntity newEntity)
        {
            return x =>
                x.EventPublicKey == newEntity.EventPublicKey &&
                x.EventKind == newEntity.EventKind;
        }
    }
}
