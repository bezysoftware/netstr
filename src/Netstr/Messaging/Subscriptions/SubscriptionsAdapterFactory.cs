namespace Netstr.Messaging.Subscriptions
{
    public interface ISubscriptionsAdapterFactory
    {
        ISubscriptionsAdapter CreateAdapter(IWebSocketAdapter webSocketAdapter);
    }

    public class SubscriptionsAdapterFactory : ISubscriptionsAdapterFactory
    {
        private readonly ILogger<SubscriptionsAdapter> logger;

        public SubscriptionsAdapterFactory(ILogger<SubscriptionsAdapter> logger)
        {
            this.logger = logger;
        }

        public ISubscriptionsAdapter CreateAdapter(IWebSocketAdapter webSocketAdapter)
        {
            return new SubscriptionsAdapter(this.logger, webSocketAdapter);
        }
    }
}
