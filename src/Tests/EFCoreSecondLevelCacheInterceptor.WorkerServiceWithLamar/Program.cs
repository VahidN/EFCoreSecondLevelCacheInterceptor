using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer.Utils;
using Lamar;
using Lamar.Microsoft.DependencyInjection;

namespace EFCoreSecondLevelCacheInterceptor.WorkerServiceWithLamar;

public static class Program
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

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)

            // Add Lamar
            .UseLamar()
            .ConfigureContainer<ServiceRegistry>((context, services) =>
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
                        .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 30));
                });

                var configuration = hostContext.Configuration;

                var connectionString = configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

                if (connectionString is null)
                {
                    throw new InvalidOperationException(message: "connectionString is null");
                }

                if (connectionString.Contains(value: "%CONTENTROOTPATH%", StringComparison.Ordinal))
                {
                    connectionString = connectionString.Replace(oldValue: "%CONTENTROOTPATH%",
                        Directory.GetCurrentDirectory(), StringComparison.Ordinal);
                }

                Console.WriteLine($"connectionString: {connectionString}");
                services.AddConfiguredMsSqlDbContext(connectionString);

                services.AddHostedService<Worker>();
            });
}