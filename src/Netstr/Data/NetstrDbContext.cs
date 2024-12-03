using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Netstr.Data
{
    public class NetstrDbContext : DbContext
    {
        public const string ReplaceableUniqueIndexName = "ReplaceableEventsIdx";
        public const string EventLookupIndexName = "EventLookupIdx";
        public const string EventIdIndexName = "EventIdIdx";
        public const string TagValueIndexName = "TagNameValueIdx";

        public NetstrDbContext(DbContextOptions<NetstrDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventEntity> Events { get; set; }
        
        public DbSet<TagEntity> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<EventEntity>(e =>
            {
                var eKind = $"\"{nameof(EventEntity.EventKind)}\"";

                e.HasKey(x => x.Id);
                e.HasMany(x => x.Tags).WithOne(x => x.Event).OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.EventId, EventIdIndexName).IsUnique();
                e.HasIndex(x => new 
                { 
                    x.EventKind,
                    x.EventPublicKey,
                    x.EventCreatedAt
                }, EventLookupIndexName);
                e.HasIndex(x => new
                {
                    x.EventPublicKey,
                    x.EventKind,
                    x.EventDeduplication
                }, ReplaceableUniqueIndexName).HasFilter(@$"
                    ({eKind} = 0) OR 
                    ({eKind} = 3) OR 
                    ({eKind} >= 10000 AND {eKind} < 20000) OR 
                    ({eKind} >= 30000 AND {eKind} < 40000)")
                .IsUnique();
            });

            builder.Entity<TagEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => new { x.Name, x.Value, x.EventId }, TagValueIndexName).IsUnique();
            });
        }
    }
}
