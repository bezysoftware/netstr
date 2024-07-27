using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging.Matching;
using Netstr.Messaging.Models;

namespace Netstr.Messaging.EventHandlers
{
    public abstract class EventHandlerBase : IEventHandler
    {
        protected readonly ILogger<EventHandlerBase> logger;
        private readonly IWebSocketAdapterCollection adapters;

        protected EventHandlerBase(ILogger<EventHandlerBase> logger, IWebSocketAdapterCollection adapters)
        {
            this.logger = logger;
            this.adapters = adapters;
        }

        public async Task HandleEventAsync(IWebSocketAdapter sender, Event e)
        {
            try
            {
                await HandleEventCoreAsync(sender, e);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueIndexViolation())
            {
                this.logger.LogInformation($"Event {e.ToStringUnique()} already exists, ignoring");
                await sender.SendOkAsync(e.Id, Messages.DuplicateEvent);
            }
        }

        public abstract bool CanHandleEvent(Event e);

        protected abstract Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e);

        protected async Task BroadcastEventAsync(Event e)
        {
            var adapters = this.adapters.GetAll();

            await Task.WhenAll(adapters.Select(x => BroadcastEventForAdapterAsync(x, e)));
        }

        private async Task BroadcastEventForAdapterAsync(IWebSocketAdapter adapter, Event e)
        {
            await adapter.LockAsync(LockType.Read, async adapter =>
            {
                var broadcast = adapter
                    .GetSubscriptions()
                    .Where(x => x.Value.IsAnyMatch(e))
                    .Select(x => adapter.SendEventAsync(x.Key, e))
                    .ToArray();

                await Task.WhenAll(broadcast);
            });
        }
    }
}
