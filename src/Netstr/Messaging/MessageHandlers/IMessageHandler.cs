using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    public interface IMessageHandler
    {
        bool CanHandleMessage(string type);

        Task HandleMessageAsync(IWebSocketAdapter adapter, JsonDocument[] parameters);
    }
}
