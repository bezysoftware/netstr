using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Netstr.Data;

namespace Netstr.Tests
{
    public class WebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                //services.RemoveAll<NetstrDbContext>();
                //services.RemoveAll<IDbContextFactory<NetstrDbContext>>();
                //services.RemoveAll<DbContextOptions>();

                //foreach (var option in services.Where(s => s.ServiceType.BaseType == typeof(DbContextOptions)).ToList())
                //{
                //    services.Remove(option);
                //}

                //services.AddDbContext<TestDbContext>();
                //services.AddDbContext<NetstrDbContext, TestDbContext>();
                //services.AddDbContextFactory<NetstrDbContext>((sp, _) => sp.GetRequiredService<NetstrDbContext>());
                services.AddScoped<NetstrDbContext>(x => TestDbContext.InitializeAndSeed(false).context);
                services.AddSingleton<IDbContextFactory<NetstrDbContext>>(x => new DbContextFactory());
            });
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
