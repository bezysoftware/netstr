using Netstr.Json;
using Netstr.Messaging.Models;
using Netstr.Messaging.Negentropy;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers.Negentropy
{
    public class NegentropyMessageHandler : IMessageHandler
    {
        public bool CanHandleMessage(string type) => type == MessageType.Negentropy.Message;

        public Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters)
        {
            if (parameters.Length < 3)
            {
                throw new UnknownMessageProcessingException($"{MessageType.Negentropy.Close} message should be an array with 3 elements");
            }

            var id = parameters[1].DeserializeRequired<string>();
            var q = parameters[2].DeserializeRequired<string>();

            try 
            { 
                adapter.Negentropy.Message(id, q);
            }
            catch (Exception ex)
            {
                throw new NegentropyProcessingException(id,  ex.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
