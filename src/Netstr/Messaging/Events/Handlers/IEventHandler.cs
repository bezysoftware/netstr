using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Handlers
{
    /// <summary>
    /// Handler of an EVENT message.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// Returns whether this handler can process given event <paramref name="e"/>.
        /// </summary>
        bool CanHandleEvent(Event e);

        /// <summary>
        /// Processes given event <paramref name="e"/>.
        /// </summary>
        Task HandleEventAsync(IWebSocketAdapter sender, Event e);
    }
}
