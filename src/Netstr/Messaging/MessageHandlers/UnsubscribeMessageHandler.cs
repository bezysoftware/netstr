using Netstr.Json;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers
{
    /// <summary>
    /// Handler which processes CLOSE messages.
    /// </summary>
    public class UnsubscribeMessageHandler : IMessageHandler
    {
        private readonly ILogger<UnsubscribeMessageHandler> logger;

        public UnsubscribeMessageHandler(ILogger<UnsubscribeMessageHandler> logger)
        {
            this.logger = logger;
        }

        public bool CanHandleMessage(string type) => type == MessageType.Close;

        public Task HandleMessageAsync(IWebSocketAdapter sender, JsonDocument[] parameters)
        {
            var id = parameters[1].DeserializeRequired<string>();

            // remove sub
            this.logger.LogInformation($"Removing subscription {id} for client {sender.Context}");
            sender.Subscriptions.RemoveById(id);

            return Task.CompletedTask;
        }
    }
}
