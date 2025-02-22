using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFMemoryCacheProviderOptions
{
    /// <summary>
    ///     Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseMemoryCacheProvider(this EFCoreSecondLevelCacheOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Settings.Services?.AddMemoryCache();
        options.Settings.Services?.TryAddSingleton<IMemoryCacheChangeTokenProvider, EFMemoryCacheChangeTokenProvider>();
        options.Settings.CacheProvider = typeof(EFMemoryCacheServiceProvider);

        return options;
    }

    /// <summary>
    ///     Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public static EFCoreSecondLevelCacheOptions UseMemoryCacheProvider(this EFCoreSecondLevelCacheOptions options,
        CacheExpirationMode expirationMode,
        TimeSpan? timeout = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.UseMemoryCacheProvider();

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return options;
    }
}