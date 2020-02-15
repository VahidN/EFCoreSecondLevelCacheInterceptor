using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils
{
    public static class DbInitialization
    {
        public static void Initialize(this IServiceScopeFactory scopeFactory)
        {
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Applies any pending migrations for the context to the database.
                // Will create the database if it does not already exist.
                context.Database.Migrate();
            }
        }
    }
}