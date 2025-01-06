using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFHybridCacheProviderOptions
{
    /// <summary>
    ///     Introduces the `EFHybridCacheProvider` to be used as the CacheProvider.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseHybridCacheProvider(this EFCoreSecondLevelCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Settings.Services?.TryAddSingleton<IEFHybridCacheDependenciesStore, EFHybridCacheDependenciesStore>();
        options.Settings.CacheProvider = typeof(EFHybridCacheProvider);

        return options;
    }

    /// <summary>
    ///     Introduces the `EFHybridCacheProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public static EFCoreSecondLevelCacheOptions UseHybridCacheProvider(this EFCoreSecondLevelCacheOptions options,
        CacheExpirationMode expirationMode,
        TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.UseHybridCacheProvider();

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}