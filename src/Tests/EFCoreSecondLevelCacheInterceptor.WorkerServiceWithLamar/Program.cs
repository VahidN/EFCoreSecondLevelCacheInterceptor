using System;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using System.IO;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;

namespace EFCoreSecondLevelCacheInterceptor.WorkerServiceWithLamar
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // How to init and seed the database
            var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>();
            serviceScope.Initialize();
            serviceScope.SeedData();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Add Lamar
                .UseLamar()
                .ConfigureContainer<Lamar.ServiceRegistry>((context, services) =>
                {
                    // Also exposes Lamar specific registrations
                    // and functionality
                    services.Scan(s =>
                    {
                        s.TheCallingAssembly();
                        s.WithDefaultConventions();
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddEFSecondLevelCache(options =>
                    {
                        options.UseMemoryCacheProvider()
                            .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
                    });

                    var configuration = hostContext.Configuration;

                    var connectionString = configuration["ConnectionStrings:ApplicationDbContextConnection"];
                    if (connectionString.Contains("%CONTENTROOTPATH%"))
                    {
                        connectionString = connectionString.Replace("%CONTENTROOTPATH%", Directory.GetCurrentDirectory());
                    }
                    Console.WriteLine($"connectionString: {connectionString}");
                    services.AddConfiguredMsSqlDbContext(connectionString);

                    services.AddHostedService<Worker>();
                });
    }
}
