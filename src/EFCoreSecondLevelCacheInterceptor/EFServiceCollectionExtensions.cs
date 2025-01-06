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
    public static IServiceCollection AddEFSecondLevelCache(this IServiceCollection services,
        Action<EFCoreSecondLevelCacheOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        services.TryAddSingleton<IEFDebugLogger, EFDebugLogger>();
        services.TryAddSingleton<IEFCacheServiceCheck, EFCacheServiceCheck>();
        services.TryAddSingleton<IEFCacheKeyPrefixProvider, EFCacheKeyPrefixProvider>();
        services.TryAddSingleton<IEFCacheKeyProvider, EFCacheKeyProvider>();
        services.TryAddSingleton<IEFCachePolicyParser, EFCachePolicyParser>();
        services.TryAddSingleton<IEFSqlCommandsProcessor, EFSqlCommandsProcessor>();
        services.TryAddSingleton<IEFCacheDependenciesProcessor, EFCacheDependenciesProcessor>();
        services.TryAddSingleton<ILockProvider, LockProvider>();
        services.TryAddSingleton<IDbCommandInterceptorProcessor, DbCommandInterceptorProcessor>();
        services.TryAddSingleton<SecondLevelCacheInterceptor>();

        ConfigOptions(services, options);

        return services;
    }

    private static void ConfigOptions(IServiceCollection services, Action<EFCoreSecondLevelCacheOptions> options)
    {
        var cacheOptions = new EFCoreSecondLevelCacheOptions
        {
            Settings =
            {
                Services = services
            }
        };

        options.Invoke(cacheOptions);

        AddHashProvider(services, cacheOptions);
        AddCacheServiceProvider(services, cacheOptions);
        AddOptions(services, cacheOptions);
    }

    private static void AddHashProvider(IServiceCollection services, EFCoreSecondLevelCacheOptions cacheOptions)
    {
        if (cacheOptions.Settings.HashProvider == null)
        {
            services.TryAddSingleton<IEFHashProvider, XxHash64Unsafe>();
        }
        else
        {
            services.TryAddSingleton(typeof(IEFHashProvider), cacheOptions.Settings.HashProvider);
        }
    }

    private static void AddOptions(IServiceCollection services, EFCoreSecondLevelCacheOptions cacheOptions)
        => services.TryAddSingleton(Options.Create(cacheOptions.Settings));

    private static void AddCacheServiceProvider(IServiceCollection services, EFCoreSecondLevelCacheOptions cacheOptions)
    {
        if (cacheOptions.Settings.CacheProvider == null)
        {
            throw new InvalidOperationException(
                message:
                "Please select an appropriate IEFCacheServiceProvider, such as `UseMemoryCacheProvider()`, `UseEasyCachingCoreProvider()`, etc. Each of these providers requires a separate nuget package. More info: https://github.com/VahidN/EFCoreSecondLevelCacheInterceptor/blob/master/README.md");
        }

        services.TryAddSingleton(typeof(IEFCacheServiceProvider), cacheOptions.Settings.CacheProvider);
    }
}