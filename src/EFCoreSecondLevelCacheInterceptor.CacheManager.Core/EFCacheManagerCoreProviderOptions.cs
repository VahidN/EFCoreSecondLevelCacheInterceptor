using System;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFCacheManagerCoreProviderOptions
{
    /// <summary>
    ///     Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider(this EFCoreSecondLevelCacheOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);

        return options;
    }

    /// <summary>
    ///     Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public static EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider(this EFCoreSecondLevelCacheOptions options,
        CacheExpirationMode expirationMode,
        TimeSpan timeout)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}