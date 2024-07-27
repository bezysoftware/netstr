using Netstr.Messaging.Models;

namespace Netstr.Messaging.EventHandlers
{
    /// <summary>
    /// Ephemeral events are not stored by the relay.
    /// </summary>
    public class EphemeralEventHandler : EventHandlerBase
    {
        public EphemeralEventHandler(
            ILogger<EphemeralEventHandler> logger,
            IWebSocketAdapterCollection adapters) 
            : base(logger, adapters)
        {
        }

        public override bool CanHandleEvent(Event e) => e.IsEphemeral();

        protected override async Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            // reply
            await sender.SendOkAsync(e.Id);

            // broadcast
            await BroadcastEventAsync(e);
        }
    }
}
