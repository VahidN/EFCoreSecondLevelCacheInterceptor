using CacheManager.Core;
using EFCoreSecondLevelCacheInterceptor;
using Issue9SQLiteInt32.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace Issue9SQLiteInt32;

public static class EFServiceProvider
{
    private static readonly Lazy<IServiceProvider> _serviceProviderBuilder =
        new(getServiceProvider, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     A lazy loaded thread-safe singleton
    /// </summary>
    public static IServiceProvider Instance { get; } = _serviceProviderBuilder.Value;

    public static T GetRequiredService<T>()
        where T : notnull
        => Instance.GetRequiredService<T>();

    public static void RunInContext(Action<ApplicationDbContext> action)
    {
        using var serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        action(context);
    }

    public static async Task RunInContextAsync(Func<ApplicationDbContext, Task> action)
    {
        using var serviceScope = GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await action(context);
    }

    private static IServiceProvider getServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.AddLogging(cfg => cfg.AddConsole().AddDebug());

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        services.AddEFSecondLevelCache(options => options

            //.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 5))
            //.UseCacheManagerCoreProvider()
            .UseHybridCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromHours(hours: 1))
            .ConfigureLogging(enable: true)
            .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(hours: 1),
                LocalCacheExpiration = TimeSpan.FromHours(hours: 1)
            };
        });

        //.AddSerializerFactory<MessagePackSerializerFactory>()

        addCacheManagerCoreRedis(services);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
                .UseSqlite(GetConnectionString(basePath, configuration));
        });

        return services.BuildServiceProvider();
    }

    public static string GetConnectionString(string basePath, IConfigurationRoot configuration)
    {
        var testsFolder = basePath.Split(new[]
        {
            "\\Issues\\"
        }, StringSplitOptions.RemoveEmptyEntries)[0];

        var contentRootPath = Path.Combine(testsFolder, path2: "Issues", path3: "Issue9SQLiteInt32");
        var connectionString = configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString is null)
        {
            throw new InvalidOperationException(message: "connectionString is null");
        }

        if (connectionString.Contains(value: "%CONTENTROOTPATH%", StringComparison.Ordinal))
        {
            connectionString =
                connectionString.Replace(oldValue: "%CONTENTROOTPATH%", contentRootPath, StringComparison.Ordinal);
        }

        Console.WriteLine($"Using {connectionString}");

        return connectionString;
    }

    private static void addCacheManagerCoreRedis(ServiceCollection services)
    {
        var jss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
#pragma warning disable CA2326
            TypeNameHandling = TypeNameHandling.Auto
#pragma warning restore CA2326
        };

        const string redisConfigurationKey = "redis";

        services.AddSingleton(new CacheConfigurationBuilder().WithJsonSerializer(jss, jss)
            .WithUpdateMode(CacheUpdateMode.Up)
            .WithRedisConfiguration(redisConfigurationKey, config =>
            {
                config.WithAllowAdmin()
                    .WithDatabase(databaseIndex: 0)
                    .WithEndpoint(host: "localhost", port: 6379)

                    // Enables keyspace notifications to react on eviction/expiration of items.
                    // Make sure that all servers are configured correctly and 'notify-keyspace-events' is at least set to 'Exe', otherwise CacheManager will not retrieve any events.
                    // See https://redis.io/topics/notifications#configuration for configuration details.
                    .EnableKeyspaceEvents();
            })
            .WithMaxRetries(retries: 100)
            .WithRetryTimeout(timeoutMillis: 50)
            .WithRedisCacheHandle(redisConfigurationKey)
            .DisableStatistics()
            .Build());

        services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
    }
}