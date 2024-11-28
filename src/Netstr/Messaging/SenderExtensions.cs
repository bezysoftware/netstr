using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    public static class SenderExtensions
    {
        public static Task SendAsync(this IWebSocketAdapter sender, object[] message)
        {
            return sender.SendAsync(MessageBatch.Single(message));
        }

        public static void Send(this IWebSocketAdapter sender, object[] message)
        {
            sender.Send(MessageBatch.Single(message));
        }

        public static Task SendOkAsync(this IWebSocketAdapter sender, string id, string message = "")
        {
            return sender.SendAsync(
            [
                MessageType.Ok,
                id, 
                true,
                message
            ]);
        }

        public static Task SendNotOkAsync(this IWebSocketAdapter sender, string id, string message)
        {
            return sender.SendAsync(
            [
                MessageType.Ok,
                id,
                false,
                message
            ]);
        }

        public static Task SendNoticeAsync(this IWebSocketAdapter sender, string message)
        {
            return sender.SendAsync(
            [
                MessageType.Notice,
                message
            ]);
        }

        public static Task SendClosedAsync(this IWebSocketAdapter sender, string id, string message = "")
        {
            return sender.SendAsync(
            [
                MessageType.Closed,
                id,
                message
            ]);
        }

        public static Task SendAuthAsync(this IWebSocketAdapter sender, string challenge)
        {
            return sender.SendAsync(
            [
                MessageType.Auth,
                challenge
            ]);
        }

        public static Task SendCountAsync(this IWebSocketAdapter sender, string id, int count)
        {
            return sender.SendAsync(
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
