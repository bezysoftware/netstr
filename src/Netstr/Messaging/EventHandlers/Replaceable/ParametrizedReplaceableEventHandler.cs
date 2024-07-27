using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Models;
using System.Linq.Expressions;

namespace Netstr.Messaging.EventHandlers.Replaceable
{
    /// <summary>
    /// Parametrized replaceable events have a unique combination of pubkey+kind+"d" tag value.
    /// </summary>
    public class ParametrizedReplaceableEventHandler : ReplaceableEventHandlerBase
    {
        public ParametrizedReplaceableEventHandler(
            ILogger<ReplaceableEventHandlerBase> logger,
            IWebSocketAdapterCollection adapters,
            IDbContextFactory<NetstrDbContext> db)
            : base(logger, adapters, db)
        {
        }

        public override bool CanHandleEvent(Event e) => e.IsParametrizedReplaceable();

        protected override Expression<Func<EventEntity, bool>> GetUniqueEntityExpression(EventEntity newEntity)
        {
            return x =>
                x.EventPublicKey == newEntity.EventPublicKey &&
                x.EventKind == newEntity.EventKind &&
                x.EventDeduplication == newEntity.EventDeduplication;
        }
    }
}
