using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    public static class SenderExtensions
    {
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

        public static Task SendEndOfStoredEventsAsync(this IWebSocketAdapter sender, string id)
        {
            return sender.SendAsync(
            [
                MessageType.EndOfStoredEvents,
                id
            ]);
        }

        public static Task SendEventAsync(this IWebSocketAdapter sender, string id, Event e)
        {
            return sender.SendAsync(
            [
                MessageType.Event,
                id,
                e
            ]);
        }

        public static async Task SendEventsAsync(this IWebSocketAdapter sender, string id, IEnumerable<Event> events)
        {
            foreach (var e in events)
            {
                await sender.SendEventAsync(id, e);
            }
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
                MessageType.Close,
                id,
                message
            ]);
        }
    }
}
