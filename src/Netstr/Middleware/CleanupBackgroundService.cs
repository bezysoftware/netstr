
using Netstr.Messaging.Events;

namespace Netstr.Middleware
{
    /// <summary>
    /// Background service which daily calls cleanup process.
    /// </summary>
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<CleanupBackgroundService> logger;
        private readonly IServiceProvider services;

        public CleanupBackgroundService(
            ILogger<CleanupBackgroundService> logger,
            IServiceProvider services)
        {
            this.logger = logger;
            this.services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            while (!stoppingToken.IsCancellationRequested)
            {
                this.logger.LogInformation("Running cleanup");

                var cleanup = this.services.GetRequiredService<ICleanupService>();
                await cleanup.RunCleanupAsync();
                
                this.logger.LogInformation("Running cleanup finished");
                
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
