using Microsoft.Extensions.Options;
using Netstr.Messaging;
using Netstr.Messaging.Negentropy;
using Netstr.Options;

namespace Netstr.Middleware
{
    /// <summary>
    /// Background service which periodically calls <see cref="NegentropyAdapter.DisposeStaleSubscriptions"/> to cleanup old negentropy subscriptions.
    /// </summary>
    public class NegentropyBackgroundWatcher : BackgroundService
    {
        private readonly IWebSocketAdapterCollection webSockets;
        private readonly IOptions<NegentropyLimitsOptions> options;
        private readonly ILogger logger;

        public NegentropyBackgroundWatcher(
            IWebSocketAdapterCollection webSockets, 
            IOptions<NegentropyLimitsOptions> options,
            ILogger<NegentropyBackgroundWatcher> logger)
        {
            this.webSockets = webSockets;
            this.options = options;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(this.options.Value.StaleSubscriptionPeriodSeconds), stoppingToken);

                this.logger.LogInformation("Checking stale negentropy subscriptions");

                // get all active websockets
                foreach (var ws in this.webSockets.GetAll().ToArray())
                {
                    ws.Negentropy.DisposeStaleSubscriptions();
                }
            } while (!stoppingToken.IsCancellationRequested);
        }
    }
}
