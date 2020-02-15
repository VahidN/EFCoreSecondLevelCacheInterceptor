using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Defines EFCoreSecondLevel's Options
    /// </summary>
    public class EFCoreSecondLevelCacheOptions
    {
        internal Type CacheProvider { get; set; }

        internal string RedisConfiguration { get; set; }

        /// <summary>
        /// You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
        /// </summary>
        /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
        public void UseCustomCacheProvider<T>() where T : IEFCacheServiceProvider
        {
            CacheProvider = typeof(T);
        }

        /// <summary>
        /// Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
        /// </summary>
        public void UseMemoryCacheProvider()
        {
            CacheProvider = typeof(EFMemoryCacheServiceProvider);
        }

        /// <summary>
        /// Introduces the built-in `EFRedisCacheServiceProvider` to be used as the CacheProvider.
        /// </summary>
        /// <param name="configuration">The string configuration to use for the multiplexer.</param>
        public void UseRedisCacheProvider(string configuration)
        {
            CacheProvider = typeof(EFRedisCacheServiceProvider);
            RedisConfiguration = configuration;
        }
    }

    /// <summary>
    /// ServiceCollection Extensions
    /// </summary>
    public static class EFServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the required services of the EFCoreSecondLevelCacheInterceptor.
        /// </summary>
        public static IServiceCollection AddEFSecondLevelCache(
            this IServiceCollection services,
            Action<EFCoreSecondLevelCacheOptions> options)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<IRedisSerializationProvider, RedisSerializationProvider>();
            services.TryAddSingleton<IReaderWriterLockProvider, ReaderWriterLockProvider>();
            services.TryAddSingleton<IEFCacheKeyProvider, EFCacheKeyProvider>();
            services.TryAddSingleton<IEFCachePolicyParser, EFCachePolicyParser>();
            services.TryAddSingleton<IEFCacheDependenciesProcessor, EFCacheDependenciesProcessor>();
            services.TryAddSingleton<IMemoryCacheChangeTokenProvider, EFMemoryCacheChangeTokenProvider>();
            services.TryAddSingleton<IDbCommandInterceptorProcessor, DbCommandInterceptorProcessor>();

            configOptions(services, options);

            return services;
        }

        private static void configOptions(IServiceCollection services, Action<EFCoreSecondLevelCacheOptions> options)
        {
            var cacheOptions = new EFCoreSecondLevelCacheOptions();
            options.Invoke(cacheOptions);

            if (cacheOptions.CacheProvider == null)
            {
                services.TryAddSingleton<IEFCacheServiceProvider, EFMemoryCacheServiceProvider>();
            }
            else
            {
                if (cacheOptions.CacheProvider == typeof(EFRedisCacheServiceProvider))
                {
                    services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(cacheOptions.RedisConfiguration));
                    services.TryAddSingleton<IRedisDbCache, RedisDbCache>();
                }
                services.TryAddSingleton(typeof(IEFCacheServiceProvider), cacheOptions.CacheProvider);
            }
        }
    }
}