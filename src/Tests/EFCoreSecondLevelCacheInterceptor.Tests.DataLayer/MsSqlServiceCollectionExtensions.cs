using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.Tests.DataLayer
{
    public static class MsSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguredMsSqlDbContext(
            this IServiceCollection services,
            string connectionString, 
            Action<DbContextOptionsBuilder> configureDbContextOptions = null)
        {
            services.AddDbContextPool<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
            {
                optionsBuilder
                    .UseSqlServer(
                        connectionString,
                        sqlServerOptionsBuilder =>
                        {
                            sqlServerOptionsBuilder
                                .CommandTimeout((int)TimeSpan.FromMinutes(3).TotalSeconds)
                                .EnableRetryOnFailure()
                                .MigrationsAssembly(typeof(MsSqlServiceCollectionExtensions).Assembly.FullName);
                        })
                    .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>());

                configureDbContextOptions?.Invoke(optionsBuilder);
            });

            return services;
        }
    }
}