using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using Netstr.Options;
using Netstr.Options.Limits;
using System.Net.WebSockets;

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
                b.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Limits:MaxPayloadSize"] = $"{MaxPayloadSize}"
                });
                b.AddInMemoryObject(EventLimits, "Limits:Events");
                b.AddInMemoryObject(SubscriptionLimits, "Limits:Subscriptions");
                b.AddInMemoryObject(NegentropyLimits, "Limits:Negentropy");
                b.AddInMemoryCollection([ KeyValuePair.Create("Auth:Mode", AuthMode.ToString())]);
            });
        }

        public SubscriptionLimits? SubscriptionLimits { get; set; }
        public EventLimits? EventLimits { get; set; }
        public NegentropyLimits? NegentropyLimits { get; set; }
        public int MaxPayloadSize { get; set; } = 524288;
        public AuthMode AuthMode { get; set; } = AuthMode.Disabled;

        public async Task<WebSocket> ConnectWebSocketAsync(AuthMode authMode = AuthMode.Disabled)
        {
            this.AuthMode = authMode;
            return await Server.CreateWebSocketClient().ConnectAsync(new Uri($"ws://localhost"), CancellationToken.None);
        }
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
