using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor.Tests.DataLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    public static class EFServiceProvider
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public static IEFCacheServiceProvider GetInMemoryCacheServiceProvider()
        {
            return GetRequiredService<IEFCacheServiceProvider>();
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

        public static IEFCacheServiceProvider GetRedisCacheServiceProvider()
        {
            var services = new ServiceCollection();

            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);

            services.AddEFSecondLevelCache(options => options.UseRedisCacheProvider(configuration["RedisConfiguration"]));
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IEFCacheServiceProvider>();
        }

        public static IServiceProvider GetConfiguredContextServiceProvider(
            bool useRedis,
            LogLevel logLevel,
            bool cacheAllQueries)
        {
            var services = new ServiceCollection();
            services.AddOptions();
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine($"Using `{basePath}` as the ContentRootPath");
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(basePath)
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .Build();
            services.AddSingleton(_ => configuration);

            var loggerProvider = new DebugLoggerProvider();
            services.AddLogging(cfg => cfg.AddConsole().AddDebug().AddProvider(loggerProvider).SetMinimumLevel(logLevel));

            services.AddEFSecondLevelCache(options =>
            {
                if (useRedis)
                {
                    options.UseRedisCacheProvider(configuration["RedisConfiguration"]);
                }
                else
                {
                    options.UseMemoryCacheProvider();
                }

                if (cacheAllQueries)
                {
                    options.CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
                }
            });

            services.AddConfiguredMsSqlDbContext(getConnectionString(basePath, configuration));

            return services.BuildServiceProvider();
        }

        private static string getConnectionString(string basePath, IConfigurationRoot configuration)
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
            bool useRedis,
            LogLevel logLevel,
            bool cacheAllQueries,
            params Action<ApplicationDbContext, DebugLoggerProvider>[] actions)
        {
            _semaphoreSlim.Wait();
            try
            {
                var serviceProvider = GetConfiguredContextServiceProvider(useRedis, logLevel, cacheAllQueries);
                serviceProvider.GetRequiredService<IEFCacheServiceProvider>().ClearAllCachedEntries();
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
            bool useRedis,
            LogLevel logLevel,
            bool cacheAllQueries,
            params Func<ApplicationDbContext, DebugLoggerProvider, Task>[] actions)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var serviceProvider = GetConfiguredContextServiceProvider(useRedis, logLevel, cacheAllQueries);
                serviceProvider.GetRequiredService<IEFCacheServiceProvider>().ClearAllCachedEntries();
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