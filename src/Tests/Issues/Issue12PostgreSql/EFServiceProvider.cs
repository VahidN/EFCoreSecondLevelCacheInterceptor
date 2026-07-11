using EFCoreSecondLevelCacheInterceptor;
using Issue12PostgreSql.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace Issue12PostgreSql;

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

        /*const string providerName = "Redis1";

        services.AddEasyCaching(o =>
        {
            o.UseRedis(cfg =>
            {
                cfg.SerializerName = "Pack";
                cfg.DBConfig.Endpoints.Add(new ServerEndPoint(host: "127.0.0.1", port: 6379));
                cfg.DBConfig.AllowAdmin = true;
                cfg.DBConfig.ConnectionTimeout = 10000;
            }, providerName);

            o.WithMessagePack(so =>
            {
                so.EnableCustomResolver = true;

                so.CustomResolvers = CompositeResolver.Create(new IMessagePackFormatter[]
                {
                    DBNullFormatter.Instance // This is necessary for the null values
                }, new IFormatterResolver[]
                {
                    NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance,
                    StandardResolverAllowPrivate.Instance, TypelessContractlessStandardResolver.Instance,
                    DynamicGenericResolver.Instance
                });
            }, name: "Pack");
        });*/

        /*services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(hours: 1),
                LocalCacheExpiration = TimeSpan.FromHours(hours: 1)
            };
        });*/

        AddFusionCache(services);

#pragma warning disable IDISP001
        var dataSource =
            new NpgsqlDataSourceBuilder(configuration[key: "ConnectionStrings:ApplicationDbContextConnection"])
                .EnableDynamicJson()
                .Build();
#pragma warning restore IDISP001

        services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
        {
            optionsBuilder.AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
                .UseNpgsql(dataSource);
        });

        return services.BuildServiceProvider();
    }

#pragma warning disable IDISP001
    private static void AddFusionCache(ServiceCollection services)
    {
        var distributedCache = new RedisCache(new RedisCacheOptions
        {
            ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                    "127.0.0.1:6379"
                },
                AllowAdmin = true,
                ConnectTimeout = 10000
            }
        });

        services.AddFusionCacheStackExchangeRedisBackplane(opt => opt.Configuration = "127.0.0.1:6379");

        var jsonSerializer = new FusionCacheSystemTextJsonSerializer();

        services.AddFusionCache()
            .WithDistributedCache(distributedCache, jsonSerializer)
            .WithSystemTextJsonSerializer()
            .WithOptions(options =>
            {
                options.DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    Duration = TimeSpan.FromMinutes(minutes: 30),
                    IsFailSafeEnabled = true,
                    FailSafeMaxDuration = TimeSpan.FromMinutes(minutes: 10),
                    FailSafeThrottleDuration = TimeSpan.FromMinutes(minutes: 10),
                    FactorySoftTimeout = TimeSpan.FromMilliseconds(milliseconds: 300),
                    FactoryHardTimeout = TimeSpan.FromMilliseconds(milliseconds: 1000),
                    DistributedCacheSoftTimeout = TimeSpan.FromMilliseconds(milliseconds: 500),
                    DistributedCacheHardTimeout = TimeSpan.FromSeconds(seconds: 5),
                    AllowBackgroundDistributedCacheOperations = true,
                    JitterMaxDuration = TimeSpan.FromSeconds(seconds: 2)
                };

                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(seconds: 5);
            })
            .WithRegisteredBackplane();

        services.AddEFSecondLevelCache(options =>
        {
            options.UseFusionCacheProvider()
                .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 30))
                .UseCacheKeyPrefix(prefix: "EF_")
                .ConfigureLogging(enable: true)
                .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1));
        });
    }
}
#pragma warning restore IDISP001