using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events
{
    public interface ICleanupService
    {
        Task RunCleanupAsync();
    }

    public class CleanupService : ICleanupService
    {
        private readonly IDbContextFactory<NetstrDbContext> db;
        private readonly ILogger<CleanupService> logger;
        private readonly IOptions<CleanupOptions> options;

        public CleanupService(
            IDbContextFactory<NetstrDbContext> db,
            ILogger<CleanupService> logger, 
            IOptions<CleanupOptions> options)
        {
            this.db = db;
            this.logger = logger;
            this.options = options;
        }

        public async Task RunCleanupAsync()
        {
            var options = this.options.Value;
            var now = DateTimeOffset.UtcNow;
            var deletedOffset = now.AddDays(-options.DeleteDeletedEventsAfterDays);
            var expiredOffset = now.AddDays(-options.DeleteExpiredEventsAfterDays);

            using var db = this.db.CreateDbContext();

            var tx = await db.Database.BeginTransactionAsync();

            // old deleted items
            await db.Events.Where(x => x.DeletedAt.HasValue && x.DeletedAt < deletedOffset).ExecuteDeleteAsync();

            // old expires items
            await db.Events.Where(x => x.EventExpiration.HasValue && x.EventExpiration < expiredOffset).ExecuteDeleteAsync();

            // kind ranges rules
            foreach (var rule in options.DeleteEventsRules)
            {
                var offset = now.AddDays(-rule.DeleteAfterDays);

                foreach (var range in rule.Kinds.Select(KindRange.Parse))
                {
                    await db.Events.Where(x => x.EventKind >= range.MinKind && x.EventKind <= range.MaxKind && x.EventCreatedAt < offset).ExecuteDeleteAsync();
                }
            }

            var count = await db.SaveChangesAsync();
            
            await tx.CommitAsync();

            this.logger.LogInformation($"Cleanup deleted {count} items");
        }
    }
}
