using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     ServiceCollection Extensions
/// </summary>
public static class EFServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the required services of the EFCoreSecondLevelCacheInterceptor.
    /// </summary>
    public static IServiceCollection AddEFSecondLevelCache(
        this IServiceCollection services,
        Action<EFCoreSecondLevelCacheOptions> options,
        bool scoped = false)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        services.AddMemoryCache();
        services.TryAddSingleton<IEFDebugLogger, EFDebugLogger>();
        services.TryAddSingleton<IEFHashProvider, XxHash64Unsafe>();
        services.TryAddSingleton<IEFCachePolicyParser, EFCachePolicyParser>();
        services.TryAddSingleton<IEFSqlCommandsProcessor, EFSqlCommandsProcessor>();
        services.TryAddSingleton<IMemoryCacheChangeTokenProvider, EFMemoryCacheChangeTokenProvider>();
        services.TryAddSingleton<ILockProvider, LockProvider>();

        if (scoped)
        {
            services.TryAddScoped<IEFCacheKeyProvider, EFCacheKeyProvider>();
            services.TryAddScoped<IEFCacheDependenciesProcessor, EFCacheDependenciesProcessor>();
            services.TryAddScoped<IDbCommandInterceptorProcessor, DbCommandInterceptorProcessor>();
            services.TryAddScoped<SecondLevelCacheInterceptor>();
        }
        else
        {
            services.TryAddSingleton<IEFCacheKeyProvider, EFCacheKeyProvider>();
            services.TryAddSingleton<IEFCacheDependenciesProcessor, EFCacheDependenciesProcessor>();
            services.TryAddSingleton<IDbCommandInterceptorProcessor, DbCommandInterceptorProcessor>();
            services.TryAddSingleton<SecondLevelCacheInterceptor>();
        }

        ConfigOptions(services, options, scoped);

        return services;
    }

    private static void ConfigOptions(IServiceCollection services, Action<EFCoreSecondLevelCacheOptions> options, bool scoped)
    {
        var cacheOptions = new EFCoreSecondLevelCacheOptions();
        options.Invoke(cacheOptions);

        if (cacheOptions.Settings.CacheProvider == null)
        {
            if (scoped)
            {
                services.TryAddScoped<IEFCacheServiceProvider, EFMemoryCacheServiceProvider>();
            }
            else
            {
                services.TryAddSingleton<IEFCacheServiceProvider, EFMemoryCacheServiceProvider>();
            }
        }
        else
        {
            if (scoped)
            {
                services.TryAddScoped(typeof(IEFCacheServiceProvider), cacheOptions.Settings.CacheProvider);
            }
            else
            {
                services.TryAddSingleton(typeof(IEFCacheServiceProvider), cacheOptions.Settings.CacheProvider);
            }
        }

        services.TryAddSingleton(Options.Create(cacheOptions.Settings));
    }
}