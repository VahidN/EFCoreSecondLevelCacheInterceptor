using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer
{
    public static class MsSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredMsSqlDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<ApplicationDbContext>(optionsBuilder => optionsBuilder.UseConfiguredMsSql(connectionString));
            return services;
        }

        public static void UseConfiguredMsSql(this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseSqlServer(
                        connectionString,
                        sqlServerOptionsBuilder =>
                        {
                            sqlServerOptionsBuilder.CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds);
                            sqlServerOptionsBuilder.EnableRetryOnFailure();
                            sqlServerOptionsBuilder.MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                        });
            optionsBuilder.AddInterceptors(new SecondLevelCacheInterceptor());
            optionsBuilder.ConfigureWarnings(warnings =>
            {
                // ...
            });
        }
    }
}