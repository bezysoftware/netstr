using Microsoft.Extensions.Options;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Handlers
{
    /// <summary>
    /// Ephemeral events are not stored by the relay.
    /// </summary>
    public class EphemeralEventHandler : EventHandlerBase
    {
        public EphemeralEventHandler(
            ILogger<EphemeralEventHandler> logger,
            IOptions<AuthOptions> auth,
            IWebSocketAdapterCollection adapters)
            : base(logger, auth, adapters)
        {
        }

        public override bool CanHandleEvent(Event e) => e.IsEphemeral();

        protected override Task HandleEventCoreAsync(IWebSocketAdapter sender, Event e)
        {
            // reply
            sender.SendOk(e.Id);

            // broadcast
            BroadcastEvent(e);

            return Task.CompletedTask;
        }
    }
}
