using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CacheManager.Core;
using Newtonsoft.Json;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    public enum TestCacheProvider
    {
        BuiltInInMemory,
        CacheManagerCoreInMemory,
        CacheManagerCoreRedis
    }

    public class SpecialTypesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?)
                    || objectType == typeof(DateTime) || objectType == typeof(DateTime?)
                    || objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?);
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => reader.Value;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("$type"); // Deserializer helper
            writer.WriteValue(value.GetType().FullName);
            writer.WritePropertyName("$value");
            writer.WriteValue(value);
            writer.WriteEndObject();
        }
    }

    public static class EFServiceProvider
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public static IEFCacheServiceProvider GetInMemoryCacheServiceProvider()
        {
            var cacheServiceProvider = GetRequiredService<IEFCacheServiceProvider>();
            return cacheServiceProvider;
        }

        public static IEFCacheServiceProvider GetCacheManagerCoreInMemory()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug());
            services.AddEFSecondLevelCache(options => options.UseCacheManagerCoreProvider());
            addCacheManagerCoreInMemory(services);
            var serviceProvider = services.BuildServiceProvider();
            var cacheProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
            cacheProvider.ClearAllCachedEntries();
            return cacheProvider;
        }

        public static IEFCacheServiceProvider GetCacheManagerCoreRedis()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug());
            services.AddEFSecondLevelCache(options => options.UseCacheManagerCoreProvider());
            addCacheManagerCoreRedis(services);
            var serviceProvider = services.BuildServiceProvider();
            var cacheProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
            cacheProvider.ClearAllCachedEntries();
            return cacheProvider;
        }

        public static T GetRequiredService<T>()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug());
            services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider());
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<T>();
        }

        public static IServiceProvider GetConfiguredContextServiceProvider(
            TestCacheProvider cacheProvider,
            LogLevel logLevel,
            bool cacheAllQueries)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);

            var loggerProvider = new DebugLoggerProvider();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug().AddProvider(loggerProvider).SetMinimumLevel(logLevel));

            services.AddEFSecondLevelCache(options =>
            {
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
                }

                if (cacheAllQueries)
                {
                    options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
                }
            });

            services.AddConfiguredMsSqlDbContext(GetConnectionString(basePath, configuration));

            return services.BuildServiceProvider();
        }

        private static void addCacheManagerCoreRedis(ServiceCollection services)
        {
            var jss = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = { new SpecialTypesConverter() }
            };

            const string redisConfigurationKey = "redis";
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                    .WithJsonSerializer(serializationSettings: jss, deserializationSettings: jss)
                    .WithUpdateMode(CacheUpdateMode.Up)
                    .WithRedisConfiguration(redisConfigurationKey, config =>
                    {
                        config.WithAllowAdmin()
                            .WithDatabase(0)
                            .WithEndpoint("localhost", 6379)
                            // Enables keyspace notifications to react on eviction/expiration of items.
                            // Make sure that all servers are configured correctly and 'notify-keyspace-events' is at least set to 'Exe', otherwise CacheManager will not retrieve any events.
                            // See https://redis.io/topics/notifications#configuration for configuration details.
                            .EnableKeyspaceEvents();
                    })
                    .WithMaxRetries(100)
                    .WithRetryTimeout(50)
                    .WithRedisCacheHandle(redisConfigurationKey)
                    .DisablePerformanceCounters()
                    .DisableStatistics()
                    .Build());
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
        }

        private static void addCacheManagerCoreInMemory(ServiceCollection services)
        {
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                                        new CacheManager.Core.ConfigurationBuilder()
                                            .WithJsonSerializer()
                                            .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
                                            .DisablePerformanceCounters()
                                            .DisableStatistics()
                                            .Build());
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
        }

        public static string GetConnectionString(string basePath, IConfigurationRoot configuration)
        {
            var testsFolder = basePath.Split(new[] { "\\Tests\\" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var contentRootPath = Path.Combine(testsFolder, "Tests", "EFCoreSecondLevelCacheInterceptor.AspNetCoreSample");
            var connectionString = configuration["ConnectionStrings:ApplicationDbContextConnection"];
            if (connectionString.Contains("%CONTENTROOTPATH%"))
            {
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", contentRootPath);
            }
            Console.WriteLine($"Using {connectionString}");
            return connectionString;
        }

        public static void RunInContext(
            TestCacheProvider cacheProvider,
            LogLevel logLevel,
            bool cacheAllQueries,
            params Action<ApplicationDbContext, DebugLoggerProvider>[] actions)
        {
            _semaphoreSlim.Wait();
            try
            {
                var serviceProvider = GetConfiguredContextServiceProvider(cacheProvider, logLevel, cacheAllQueries);
                var cacheServiceProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
                cacheServiceProvider.ClearAllCachedEntries();
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
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public static async Task RunInContextAsync(
            TestCacheProvider cacheProvider,
            LogLevel logLevel,
            bool cacheAllQueries,
            params Func<ApplicationDbContext, DebugLoggerProvider, Task>[] actions)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var serviceProvider = GetConfiguredContextServiceProvider(cacheProvider, logLevel, cacheAllQueries);
                var cacheServiceProvider = serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
                cacheServiceProvider.ClearAllCachedEntries();
                using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    foreach (var action in actions)
                    {
                        using (var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                        {
                            await action(context, (DebugLoggerProvider)serviceProvider.GetRequiredService<ILoggerProvider>());
                        }
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
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
}