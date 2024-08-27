using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Options;

namespace Netstr.Tests
{
    public class WebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<NetstrDbContext>(x => TestDbContext.InitializeAndSeed(false).context);
                services.AddSingleton<IDbContextFactory<NetstrDbContext>>(x => new DbContextFactory());
            });

            builder.ConfigureAppConfiguration((ctx, b) =>
            {
                b.AddInMemoryObject(Limits, "Limits");
                b.AddInMemoryCollection([ KeyValuePair.Create("Auth:Mode", AuthMode.ToString())]);
            });
        }

        public LimitsOptions? Limits { get; set; }
        public AuthMode AuthMode { get; set; } = AuthMode.Disabled;
    }

    public class DbContextFactory : IDbContextFactory<NetstrDbContext>
    {
        private readonly DbContextOptions<NetstrDbContext> options;
        
        public DbContextFactory()
        {
            this.options = TestDbContext.InitializeAndSeed(false).options;
        }

        public NetstrDbContext CreateDbContext()
        {
            return new TestDbContext(this.options);
        }
    }
}
