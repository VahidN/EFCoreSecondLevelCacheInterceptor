using System;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFFusionCacheProviderOptions
{
    /// <summary>
    ///     Introduces the `EFFusionCacheProvider` to be used as the CacheProvider.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="namedCache">
    ///     Name of the named cache! If it's not specified, the default cache will be used. It must match
    ///     the one provided during registration (services.AddFusionCache("__name__")).
    /// </param>
    public static EFCoreSecondLevelCacheOptions UseFusionCacheProvider(this EFCoreSecondLevelCacheOptions options,
        string? namedCache = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Settings.CacheProvider = typeof(EFFusionCacheProvider);

        options.Settings.AdditionalData = new EFFusionCacheConfigurationOptions
        {
            NamedCache = namedCache
        };

        return options;
    }

    /// <summary>
    ///     Introduces the `EFFusionCacheProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="namedCache">
    ///     Name of the named cache! If it's not specified, the default cache will be used. It must match
    ///     the one provided during registration (services.AddFusionCache("__name__")).
    /// </param>
    public static EFCoreSecondLevelCacheOptions UseFusionCacheProvider(this EFCoreSecondLevelCacheOptions options,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string? namedCache = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.UseFusionCacheProvider(namedCache);

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}