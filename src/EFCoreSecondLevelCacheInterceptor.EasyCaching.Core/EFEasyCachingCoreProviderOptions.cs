using System;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFEasyCachingCoreProviderOptions
{
    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="providerName">Selected caching provider name.</param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public static EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(this EFCoreSecondLevelCacheOptions options,
        string providerName,
        bool isHybridCache = false)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        options.Settings.ProviderName = providerName;
        options.Settings.IsHybridCache = isHybridCache;

        return options;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="providerName">
    ///     Selected caching provider name.
    ///     This option will let you choose a different redis database for your current tenant.
    ///     <![CDATA[ Such as: (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"]; ]]>
    /// </param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public static EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(this EFCoreSecondLevelCacheOptions options,
        Func<IServiceProvider, EFCacheKey?, string> providerName,
        bool isHybridCache = false)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        options.Settings.CacheProviderName = providerName;
        options.Settings.IsHybridCache = isHybridCache;

        return options;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="providerName">Selected caching provider name.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public static EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(this EFCoreSecondLevelCacheOptions options,
        string providerName,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        bool isHybridCache = false)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        options.Settings.ProviderName = providerName;
        options.Settings.IsHybridCache = isHybridCache;

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="providerName">
    ///     Selected caching provider name.
    ///     This option will let you choose a different redis database for your current tenant.
    ///     <![CDATA[ Such as: (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"]; ]]>
    /// </param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public static EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(this EFCoreSecondLevelCacheOptions options,
        Func<IServiceProvider, EFCacheKey?, string> providerName,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        bool isHybridCache = false)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        options.Settings.CacheProviderName = providerName;
        options.Settings.IsHybridCache = isHybridCache;

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}