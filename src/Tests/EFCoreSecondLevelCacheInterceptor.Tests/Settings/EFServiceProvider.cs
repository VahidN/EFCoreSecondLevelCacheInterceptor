using AsyncKeyedLock;
using CacheManager.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.InMemory;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using ZiggyCreatures.Caching.Fusion;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public static class EFServiceProvider
{
    private static readonly AsyncNonKeyedLocker _locker = new();

    public static IEFCacheServiceProvider GetCacheServiceProvider(TestCacheProvider provider)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging(cfg => cfg.AddConsole().AddDebug());

        switch (provider)
        {
            case TestCacheProvider.BuiltInInMemory:
                services.AddEFSecondLevelCache(options
                    => options.UseMemoryCacheProvider()
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                break;
            case TestCacheProvider.CacheManagerCoreInMemory:
                services.AddEFSecondLevelCache(options
                    => options.UseCacheManagerCoreProvider()
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                addCacheManagerCoreInMemory(services);

                break;
            case TestCacheProvider.CacheManagerCoreRedis:
                services.AddEFSecondLevelCache(options
                    => options.UseCacheManagerCoreProvider()
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                addCacheManagerCoreRedis(services);

                break;
            case TestCacheProvider.EasyCachingCoreInMemory:
                const string providerName1 = "InMemory1";

                services.AddEFSecondLevelCache(options
                    => options.UseEasyCachingCoreProvider(providerName1)
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                addEasyCachingCoreInMemory(services, providerName1);

                break;
            case TestCacheProvider.EasyCachingCoreRedis:
                const string providerName2 = "Redis1";

                services.AddEFSecondLevelCache(options
                    => options.UseEasyCachingCoreProvider(providerName2)
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                addEasyCachingCoreRedis(services, providerName2);

                break;
            case TestCacheProvider.EasyCachingCoreHybrid:
                const string providerName3 = "Hybrid1";

                services.AddEFSecondLevelCache(options
                    => options.UseEasyCachingCoreProvider(providerName3, isHybridCache: true)
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                addEasyCachingCoreHybrid(services, providerName3);

                break;
            case TestCacheProvider.FusionCache:
                AddFusionCache(services);

                services.AddEFSecondLevelCache(options
                    => options.UseFusionCacheProvider()
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                break;
            case TestCacheProvider.StackExchangeRedis:

                var redisOptions = new ConfigurationOptions
                {
                    EndPoints = new EndPointCollection
                    {
                        {
                            "127.0.0.1", 6379
                        }
                    },
                    AllowAdmin = true,
                    ConnectTimeout = 10000
                };

                services.AddEFSecondLevelCache(options
                    => options.UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(minutes: 5))
                        .ConfigureLogging(enable: true)
                        .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, message: null);
        }

        using var serviceProvider = services.BuildServiceProvider();
        var cacheProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();

        try
        {
            cacheProvider.ClearAllCachedEntries();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return cacheProvider;
    }

    private static void AddFusionCache(ServiceCollection services)
        => services.AddFusionCache()
            .WithOptions(options =>
            {
                options.DefaultEntryOptions = new FusionCacheEntryOptions
                {
                    // CACHE DURATION
                    Duration = TimeSpan.FromMinutes(minutes: 1),

                    // FAIL-SAFE OPTIONS
                    IsFailSafeEnabled = true,
                    FailSafeMaxDuration = TimeSpan.FromHours(hours: 2),
                    FailSafeThrottleDuration = TimeSpan.FromSeconds(seconds: 30),

                    // FACTORY TIMEOUTS
                    FactorySoftTimeout = TimeSpan.FromMilliseconds(milliseconds: 500),
                    FactoryHardTimeout = TimeSpan.FromMilliseconds(milliseconds: 1500),

                    // DISTRIBUTED CACHE
                    DistributedCacheSoftTimeout = TimeSpan.FromSeconds(seconds: 10),
                    DistributedCacheHardTimeout = TimeSpan.FromSeconds(seconds: 20),
                    AllowBackgroundDistributedCacheOperations = true,

                    // JITTERING
                    JitterMaxDuration = TimeSpan.FromSeconds(seconds: 2)
                };

                // DISTIBUTED CACHE CIRCUIT-BREAKER
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(seconds: 2);

                // CUSTOM LOG LEVELS
                options.FailSafeActivationLogLevel = LogLevel.Debug;
                options.SerializationErrorsLogLevel = LogLevel.Warning;
                options.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.DistributedCacheErrorsLogLevel = LogLevel.Error;
                options.FactorySyntheticTimeoutsLogLevel = LogLevel.Debug;
                options.FactoryErrorsLogLevel = LogLevel.Error;
            });

    public static T GetRequiredService<T>()
        where T : notnull
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging(cfg => cfg.AddConsole().AddDebug());

        services.AddEFSecondLevelCache(options
            => options.UseMemoryCacheProvider()
                .ConfigureLogging(enable: true)
                .UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1)));

        using var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<T>();
    }

    public static IServiceProvider GetConfiguredContextServiceProvider(TestCacheProvider cacheProvider,
        LogLevel logLevel,
        bool cacheAllQueries)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        var basePath = Directory.GetCurrentDirectory();
        Console.WriteLine($"Using `{basePath}` as the ContentRootPath");

        var configuration = new ConfigurationBuilder().SetBasePath(basePath)
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton(_ => configuration);

#pragma warning disable IDISP001
        var loggerProvider = new DebugLoggerProvider();
#pragma warning restore IDISP001
        services.AddLogging(cfg => cfg.AddConsole().AddDebug().AddProvider(loggerProvider).SetMinimumLevel(logLevel));

        services.AddEFSecondLevelCache(options =>
        {
            options.ConfigureLogging(enable: true).UseDbCallsIfCachingProviderIsDown(TimeSpan.FromMinutes(minutes: 1));

            switch (cacheProvider)
            {
                case TestCacheProvider.BuiltInInMemory:
                    options.UseMemoryCacheProvider();

                    break;
                case TestCacheProvider.CacheManagerCoreInMemory:
                    options.UseCacheManagerCoreProvider();
                    addCacheManagerCoreInMemory(services);

                    break;
                case TestCacheProvider.CacheManagerCoreRedis:
                    options.UseCacheManagerCoreProvider();
                    addCacheManagerCoreRedis(services);

                    break;
                case TestCacheProvider.EasyCachingCoreInMemory:
                    const string providerName1 = "InMemory-1";
                    options.UseEasyCachingCoreProvider(providerName1);
                    addEasyCachingCoreInMemory(services, providerName1);

                    break;
                case TestCacheProvider.EasyCachingCoreRedis:
                    const string providerName2 = "Redis-1";
                    options.UseEasyCachingCoreProvider(providerName2);
                    addEasyCachingCoreRedis(services, providerName2);

                    break;
                case TestCacheProvider.EasyCachingCoreHybrid:
                    const string providerName3 = "Hybrid1";
                    options.UseEasyCachingCoreProvider(providerName3, isHybridCache: true);
                    addEasyCachingCoreHybrid(services, providerName3);

                    break;
                case TestCacheProvider.FusionCache:
                    AddFusionCache(services);
                    options.UseFusionCacheProvider();

                    break;
                case TestCacheProvider.StackExchangeRedis:
                    var redisOptions = new ConfigurationOptions
                    {
                        EndPoints = new EndPointCollection
                        {
                            {
                                "127.0.0.1", 6379
                            }
                        },
                        AllowAdmin = true,
                        ConnectTimeout = 10000
                    };

                    options.UseStackExchangeRedisCacheProvider(redisOptions, TimeSpan.FromMinutes(minutes: 5));

                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (cacheAllQueries)
            {
                options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(minutes: 30));
            }
        });

        services.AddConfiguredMsSqlDbContext(GetConnectionString(basePath, configuration));

#pragma warning disable IDISP005
        return services.BuildServiceProvider();
#pragma warning restore IDISP005
    }

    private static void addEasyCachingCoreHybrid(ServiceCollection services, string hybridProviderName)
    {
        const string redisProvider = "redis";
        const string memoryProvider = "memory";

        // More info: https://easycaching.readthedocs.io/en/latest/HybridCachingProvider/#how-to-use
        services.AddEasyCaching(option =>
        {
            option.UseRedis(config =>
                {
                    config.DBConfig.AllowAdmin = true;
                    config.DBConfig.Endpoints.Add(new ServerEndPoint(host: "127.0.0.1", port: 6379));
                    config.SerializerName = "MySerializer";
                    config.DBConfig.ConnectionTimeout = 10000;
                }, redisProvider)
                .WithMessagePack(so =>
                {
                    so.EnableCustomResolver = true;

                    so.CustomResolvers = CompositeResolver.Create(new IMessagePackFormatter[]
                    {
                        DBNullFormatter.Instance // This is necessary for the null values
                    }, new IFormatterResolver[]
                    {
                        NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance,
                        StandardResolverAllowPrivate.Instance
                    });
                }, name: "MySerializer")

                //.WithSystemTextJson("MySerializer")
                .UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,

                        // total count of cache items, default value is 10000
                        SizeLimit = 100,

                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = false,

                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = false
                    };

                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;

                    // whether enable logging, default is false
                    config.EnableLogging = false;

                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;

                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                }, memoryProvider)
                .UseHybrid(config =>
                {
                    config.TopicName = "topic";
                    config.LocalCacheProviderName = memoryProvider;
                    config.DistributedCacheProviderName = redisProvider;
                }, hybridProviderName)
                .WithRedisBus(busConf =>
                {
                    busConf.Endpoints.Add(new ServerEndPoint(host: "127.0.0.1", port: 6379));
                    busConf.AllowAdmin = true;
                });
        });
    }

    private static void addEasyCachingCoreRedis(ServiceCollection services, string providerName)
        =>

            // It needs <PackageReference Include="EasyCaching.Redis" Version="0.8.8" />
            // More info: https://easycaching.readthedocs.io/en/latest/Redis/
            services.AddEasyCaching(option =>
            {
                option.UseRedis(config =>
                    {
                        config.DBConfig.AllowAdmin = true;
                        config.DBConfig.Endpoints.Add(new ServerEndPoint(host: "127.0.0.1", port: 6379));
                        config.SerializerName = "MySerializer";
                        config.DBConfig.ConnectionTimeout = 10000;
                    }, providerName)
                    .WithMessagePack(so =>
                    {
                        so.EnableCustomResolver = true;

                        so.CustomResolvers = CompositeResolver.Create(new IMessagePackFormatter[]
                        {
                            DBNullFormatter.Instance // This is necessary for the null values
                        }, new IFormatterResolver[]
                        {
                            NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance,
                            StandardResolverAllowPrivate.Instance
                        });
                    }, name: "MySerializer");
            });

    private static void addEasyCachingCoreInMemory(ServiceCollection services, string providerName)
        =>

            // It needs <PackageReference Include="EasyCaching.InMemory" Version="0.8.8" />
            // More info: https://easycaching.readthedocs.io/en/latest/In-Memory/
            services.AddEasyCaching(options =>
            {
                // use memory cache with your own configuration
                options.UseInMemory(config =>
                {
                    config.DBConfig = new InMemoryCachingOptions
                    {
                        // scan time, default value is 60s
                        ExpirationScanFrequency = 60,

                        // total count of cache items, default value is 10000
                        SizeLimit = 100,

                        // enable deep clone when reading object from cache or not, default value is true.
                        EnableReadDeepClone = false,

                        // enable deep clone when writing object to cache or not, default value is false.
                        EnableWriteDeepClone = false
                    };

                    // the max random second will be added to cache's expiration, default value is 120
                    config.MaxRdSecond = 120;

                    // whether enable logging, default is false
                    config.EnableLogging = false;

                    // mutex key's alive time(ms), default is 5000
                    config.LockMs = 5000;

                    // when mutex key alive, it will sleep some time, default is 300
                    config.SleepMs = 300;
                }, providerName);
            });

    private static void addCacheManagerCoreRedis(ServiceCollection services)
    {
        var jss = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
#pragma warning disable CA2326
            TypeNameHandling = TypeNameHandling.Auto,
#pragma warning restore CA2326
            Converters =
            {
                new SpecialTypesConverter()
            }
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

    private static void addCacheManagerCoreInMemory(ServiceCollection services)
    {
        services.AddSingleton(new CacheConfigurationBuilder().WithJsonSerializer()
            .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
            .DisableStatistics()
            .Build());

        services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
    }

    public static string GetConnectionString(string basePath, IConfigurationRoot configuration)
    {
        var testsFolder = basePath.Split(new[]
        {
            "\\Tests\\"
        }, StringSplitOptions.RemoveEmptyEntries)[0];

        var contentRootPath = Path.Combine(testsFolder, path2: "Tests",
            path3: "EFCoreSecondLevelCacheInterceptor.AspNetCoreSample");

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

    public static void RunInContext(TestCacheProvider cacheProvider,
        LogLevel logLevel,
        bool cacheAllQueries,
        params Action<ApplicationDbContext, DebugLoggerProvider>[] actions)
    {
        using (_locker.Lock())
        {
#pragma warning disable IDISP001
            var serviceProvider = GetConfiguredContextServiceProvider(cacheProvider, logLevel, cacheAllQueries);
#pragma warning restore IDISP001

            try
            {
                var cacheServiceProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
                cacheServiceProvider.ClearAllCachedEntries();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                foreach (var action in actions)
                {
                    using (var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        action(context, (DebugLoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>());
                    }
                }
            }
        }
    }

    public static async Task RunInContextAsync(TestCacheProvider cacheProvider,
        LogLevel logLevel,
        bool cacheAllQueries,
        params Func<ApplicationDbContext, DebugLoggerProvider, Task>[] actions)
    {
        using (await _locker.LockAsync())
        {
#pragma warning disable IDISP001
            var serviceProvider = GetConfiguredContextServiceProvider(cacheProvider, logLevel, cacheAllQueries);
#pragma warning restore IDISP001
            var cacheServiceProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
            cacheServiceProvider.ClearAllCachedEntries();

            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                foreach (var action in actions)
                {
                    using (var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                    {
                        await action(context,
                            (DebugLoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>());
                    }
                }
            }
        }
    }

    public static void ExecuteInParallel(Action test, int count = 40)
    {
        var tests = new Action[count];

        for (var i = 0; i < count; i++)
        {
            tests[i] = test;
        }

        Parallel.Invoke(tests);
    }
}