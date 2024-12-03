using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Netstr.Data;
using Netstr.Messaging.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Netstr.Tests
{
    public class TestDbContext : NetstrDbContext
    {
        public TestDbContext(DbContextOptions<NetstrDbContext> options) : base(options)
        {
        }

        public static (SqliteConnection connection, TestDbContext context, DbContextOptions<NetstrDbContext> options) InitializeAndSeed(bool seed = true, string file = "./Resources/Events.json")
        {
            // SQLite connection
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<NetstrDbContext>().UseSqlite(connection).Options;

            // DB Context
            var context = new TestDbContext(options);
            context.Database.EnsureCreated();

            if (seed)
            {
                // Seed with data
                var json = File.ReadAllText("./Resources/Events.json");
                var events = JsonSerializer.Deserialize<Event[]>(json) ?? throw new InvalidOperationException("Couldn't deserialize events");
                var entities = events.Select(x => x.ToEntity(DateTimeOffset.UtcNow));

                context.AddRange(entities);
                context.SaveChanges();
            }

            return (connection, context, options);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.LogTo(x => Debug.WriteLine(x));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                                || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        builder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
        }
    }
}
