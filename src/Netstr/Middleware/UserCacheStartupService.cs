using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Messaging;
using Netstr.Messaging.Models;

namespace Netstr.Middleware
{
    /// <summary>
    /// Initialize cache when the app starts.
    /// </summary>
    public class UserCacheStartupService : IHostedService
    {
        private readonly ILogger<UserCacheStartupService> logger;
        private readonly IDbContextFactory<NetstrDbContext> db;
        private readonly IUserCache cache;

        public UserCacheStartupService
            (ILogger<UserCacheStartupService> logger, 
            IDbContextFactory<NetstrDbContext> db,
            IUserCache cache)
        {
            this.logger = logger;
            this.db = db;
            this.cache = cache;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Initializing user cache started");
            
            using var db = this.db.CreateDbContext();

            // for each user take their last 'request to vanish' event
            var events = await db.Events
                .AsNoTracking()
                .GroupBy(x => new { x.EventKind, x.EventPublicKey })
                .Where(x => x.Key.EventKind == EventKind.RequestToVanish)
                .Select(x => new { x.Key.EventPublicKey, VanishedAt = x.Max(x => x.EventCreatedAt) })
                .ToArrayAsync(cancellationToken);

            var users = events
                .Select(x => new User { PublicKey = x.EventPublicKey, LastVanished = x.VanishedAt })
                .ToArray();

            this.cache.Initialize(users);

            this.logger.LogInformation($"Initializing user cache done with {users.Length} users");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
