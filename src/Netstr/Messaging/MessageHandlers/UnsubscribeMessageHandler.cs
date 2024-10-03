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

        public Task HandleMessageAsync(IWebSocketAdapter sender, JsonDocument[] parameters)
        {
            var id = parameters[1].DeserializeRequired<string>();

            // remove sub
            sender.RemoveSubscription(id);

            return Task.CompletedTask;
        }
    }
}
