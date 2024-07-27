using Microsoft.EntityFrameworkCore;

namespace Netstr.Extensions
{
    public static class DbExtensions
    {
        /// <summary>
        /// Ensures migrations are run for given <see cref="DbContext"/> during startup.
        /// </summary>
        public static IApplicationBuilder EnsureDbContextMigrations<T>(this IApplicationBuilder app) where T : DbContext
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<T>();

            context.Database.Migrate();

            return app;
        }
    }
}
