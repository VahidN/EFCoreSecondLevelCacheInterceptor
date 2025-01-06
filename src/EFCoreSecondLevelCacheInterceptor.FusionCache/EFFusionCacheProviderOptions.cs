using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFFusionCacheProviderOptions
{
    /// <summary>
    ///     Introduces the `EFFusionCacheProvider` to be used as the CacheProvider.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseFusionCacheProvider(this EFCoreSecondLevelCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Settings.Services?.TryAddSingleton<IEFFusionCacheDependenciesStore, EFFusionCacheDependenciesStore>();
        options.Settings.CacheProvider = typeof(EFFusionCacheProvider);

        return options;
    }

    /// <summary>
    ///     Introduces the `EFFusionCacheProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public static EFCoreSecondLevelCacheOptions UseFusionCacheProvider(this EFCoreSecondLevelCacheOptions options,
        CacheExpirationMode expirationMode,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.UseFusionCacheProvider();

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}