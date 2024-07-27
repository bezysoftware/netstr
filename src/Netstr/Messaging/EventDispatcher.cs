using Netstr.Messaging.EventHandlers;
using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    /// <summary>
    /// Dispatches EVENT message to someone who can handle it.
    /// </summary>
    public interface IEventDispatcher
    {
        Task DispatchEventAsync(IWebSocketAdapter sender, Event e);
    }

    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> logger;
        private readonly IEnumerable<IEventHandler> eventHandlers;

        public EventDispatcher(ILogger<EventDispatcher> logger, IEnumerable<IEventHandler> eventHandlers)
        {
            this.logger = logger;
            this.eventHandlers = eventHandlers;
        }

        public async Task DispatchEventAsync(IWebSocketAdapter sender, Event e)
        {
            var handler = this.eventHandlers.FirstOrDefault(x => x.CanHandleEvent(e));

            if (handler == null)
            {
                this.logger.LogWarning($"Couldn't find an event handler for event {e.Id}, kind {e.Kind}");
                return;
            }

            await handler.HandleEventAsync(sender, e);
        }
    }
}
