using Netstr.Extensions;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes CLOSE messages.
    /// </summary>
    public class UnsubscribeMessageHandler : IMessageHandler
    {
        public bool CanHandleMessage(string type) => type == MessageType.Close;

        public async Task HandleMessageAsync(IWebSocketAdapter sender, JsonDocument[] parameters)
        {
            var id = parameters[1].DeserializeRequired<string>();

            await sender.LockAsync(LockType.Write, adapter =>
            {
                // remove sub
                adapter.RemoveSubscription(id);

                return Task.CompletedTask;
            });
        }
    }
}
