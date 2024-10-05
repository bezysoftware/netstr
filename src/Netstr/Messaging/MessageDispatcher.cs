using System.Text.Json;
using Netstr.Messaging.MessageHandlers;

namespace Netstr.Messaging
{
    public interface IMessageDispatcher
    {
        Task DispatchMessageAsync(IWebSocketAdapter sender, string message);
    }

    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly ILogger<MessageDispatcher> logger;
        private readonly IEnumerable<IMessageHandler> messageHandlers;

        public MessageDispatcher(
            ILogger<MessageDispatcher> logger,
            IEnumerable<IMessageHandler> messageHandlers)
        {
            this.logger = logger;
            this.messageHandlers = messageHandlers;
        }

        public async Task DispatchMessageAsync(IWebSocketAdapter sender, string message)
        {
            try
            {
                var (handler, parts) = FindHandler(message);

                await handler.HandleMessageAsync(sender, parts);
            }
            catch (MessageProcessingException ex)
            {
                this.logger.LogWarning(ex, $"Failed to process message: {message}");
                await sender.SendAsync(ex.GetSenderReply());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error while processing message: {message}");
                await sender.SendNoticeAsync(Messages.ErrorInternal);
            }
        }

        public (IMessageHandler, JsonDocument[]) FindHandler(string message)
        {
            var parts = JsonSerializer.Deserialize<JsonDocument[]>(message);
            var typePart = parts?.FirstOrDefault();

            if (parts == null || typePart == null)
            {
                this.logger.LogWarning($"Couldn't get message type");
                throw new MessageProcessingException(Messages.CannotParseMessage);
            }

            var type = typePart.Deserialize<string>() ?? "";
            var handler = this.messageHandlers.FirstOrDefault(x => x.CanHandleMessage(type));

            if (handler == null)
            {
                this.logger.LogWarning($"No handler for message type {type}");
                throw new MessageProcessingException($"{Messages.CannotProcessMessageType} {type}");
            }

            return (handler, parts);
        }
    }
}
