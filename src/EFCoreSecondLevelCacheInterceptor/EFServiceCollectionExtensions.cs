using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
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
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddMemoryCache();
            services.TryAddSingleton<IEFDebugLogger, EFDebugLogger>();
            services.TryAddSingleton<IEFHashProvider, XxHashUnsafe>();
            services.TryAddSingleton<IReaderWriterLockProvider, ReaderWriterLockProvider>();
            services.TryAddSingleton<IEFCacheKeyProvider, EFCacheKeyProvider>();
            services.TryAddSingleton<IEFCachePolicyParser, EFCachePolicyParser>();
            services.TryAddSingleton<IEFSqlCommandsProcessor, EFSqlCommandsProcessor>();
            services.TryAddSingleton<IEFCacheDependenciesProcessor, EFCacheDependenciesProcessor>();
            services.TryAddSingleton<IMemoryCacheChangeTokenProvider, EFMemoryCacheChangeTokenProvider>();
            services.TryAddSingleton<IDbCommandInterceptorProcessor, DbCommandInterceptorProcessor>();
            services.TryAddSingleton<SecondLevelCacheInterceptor>();

            configOptions(services, options);

            return services;
        }

        private static void configOptions(IServiceCollection services, Action<EFCoreSecondLevelCacheOptions> options)
        {
            var cacheOptions = new EFCoreSecondLevelCacheOptions();
            options.Invoke(cacheOptions);

            if (cacheOptions.Settings.CacheProvider == null)
            {
                services.TryAddSingleton<IEFCacheServiceProvider, EFMemoryCacheServiceProvider>();
            }
            else
            {
                services.TryAddSingleton(typeof(IEFCacheServiceProvider), cacheOptions.Settings.CacheProvider);
            }

            services.TryAddSingleton(Options.Create(cacheOptions.Settings));
        }
    }
}