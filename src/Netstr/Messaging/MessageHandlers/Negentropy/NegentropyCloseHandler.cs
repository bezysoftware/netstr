using Netstr.Json;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers.Negentropy
{
    public class NegentropyCloseHandler : IMessageHandler
    {
        public bool CanHandleMessage(string type) => type == MessageType.Negentropy.Close;

        public Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new UnknownMessageProcessingException($"{MessageType.Negentropy.Close} message should be an array with 2 elements");
            }

            var id = parameters[1].DeserializeRequired<string>();

            adapter.Negentropy.Close(id);

            return Task.CompletedTask;
        }
    }
}
