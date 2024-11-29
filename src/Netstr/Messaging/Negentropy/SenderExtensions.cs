using Netstr.Messaging.Models;

namespace Netstr.Messaging.Negentropy
{
    public static class SenderExtensions
    {
        public static void SendNegentropyError(this IWebSocketAdapter sender, string id, string message)
        {
            sender.Send(
            [
                MessageType.Negentropy.Error,
                id,
                message
            ]);
        }

        public static void SendNegentropyMessage(this IWebSocketAdapter sender, string id, string message)
        {
            sender.Send(
            [
                MessageType.Negentropy.Message,
                id,
                message
            ]);
        }
    }
}
