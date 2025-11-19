using EFCoreSecondLevelCacheInterceptor;
using Issue123WithMessagePack.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NewtonsoftJson;

namespace Issue123WithMessagePack;

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

        services.AddLogging(cfg => cfg.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Debug));

#pragma warning disable S125
        /*
         const string providerName = "Redis1";
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
#pragma warning restore S125

        using var distributedCache = new RedisCache(new RedisCacheOptions
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

        var jsonSerializer = new FusionCacheNewtonsoftJsonSerializer();

        services.AddFusionCache()
            .WithDistributedCache(distributedCache, jsonSerializer)
            .WithNewtonsoftJsonSerializer()
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
            });

        services.AddEFSecondLevelCache(o =>
        {
            // o.UseEasyCachingCoreProvider(providerName).ConfigureLogging(enable: true) 

#pragma warning disable S125
            /*o.UseStackExchangeRedisCacheProvider(new ConfigurationOptions
            {
                EndPoints = new EndPointCollection
                {
                    {
                        "127.0.0.1", 6379
                    }
                },
                AllowAdmin = true,
                ConnectTimeout = 10000
            }, TimeSpan.FromMinutes(minutes: 5));*/
#pragma warning restore S125

            o.UseFusionCacheProvider();

            o.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 10));
            o.UseCacheKeyPrefix(prefix: "EF_");

            // Fallback on db if the caching provider fails.
            o.UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1));
        });

        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>())
                .UseSqlServer(GetConnectionString(basePath, configuration))
                .LogTo(sql => Console.WriteLine(sql));
        });

        return services.BuildServiceProvider();
    }

    public static string? GetConnectionString(string basePath, IConfigurationRoot configuration)
    {
        var testsFolder = basePath.Split(new[]
        {
            "\\Issues\\"
        }, StringSplitOptions.RemoveEmptyEntries)[0];

        var contentRootPath = Path.Combine(testsFolder, path2: "Issues", path3: "Issue123WithMessagePack");
        var connectionString = configuration[key: "ConnectionStrings:ApplicationDbContextConnection"];

        if (connectionString?.Contains(value: "%CONTENTROOTPATH%", StringComparison.Ordinal) == true)
        {
            connectionString =
                connectionString.Replace(oldValue: "%CONTENTROOTPATH%", contentRootPath, StringComparison.Ordinal);
        }

        Console.WriteLine($"Using {connectionString}");

        return connectionString;
    }
}