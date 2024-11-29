using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    public static class SenderExtensions
    {
        public static void Send(this IWebSocketAdapter sender, object[] message)
        {
            sender.Send(MessageBatch.Single(message));
        }

        public static void SendOk(this IWebSocketAdapter sender, string id, string message = "")
        {
            sender.Send(
            [
                MessageType.Ok,
                id, 
                true,
                message
            ]);
        }

        public static void SendNotOk(this IWebSocketAdapter sender, string id, string message)
        {
            sender.Send(
            [
                MessageType.Ok,
                id,
                false,
                message
            ]);
        }

        public static void SendNotice(this IWebSocketAdapter sender, string message)
        {
            sender.Send(
            [
                MessageType.Notice,
                message
            ]);
        }

        public static void SendClosed(this IWebSocketAdapter sender, string id, string message = "")
        {
            sender.Send(
            [
                MessageType.Closed,
                id,
                message
            ]);
        }

        public static void SendAuth(this IWebSocketAdapter sender, string challenge)
        {
            sender.Send(
            [
                MessageType.Auth,
                challenge
            ]);
        }

        public static void SendCount(this IWebSocketAdapter sender, string id, int count)
        {
            sender.Send(
            [
                MessageType.Count,
                id,
                new {
                    count
                }
            ]);
        }
    }
}
