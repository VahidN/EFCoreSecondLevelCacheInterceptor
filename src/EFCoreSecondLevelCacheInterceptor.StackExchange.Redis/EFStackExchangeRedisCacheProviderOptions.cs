using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public static class EFStackExchangeRedisCacheProviderOptions
{
    /// <summary>
    ///     Introduces the `EFStackExchangeRedisCacheProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseStackExchangeRedisCacheProvider(
        this EFCoreSecondLevelCacheOptions options,
        ConfigurationOptions configurationOptions,
        TimeSpan timeout)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        SetOptions(options, timeout, new EFRedisCacheConfigurationOptions
        {
            ConfigurationOptions = configurationOptions
        });

        return options;
    }

    /// <summary>
    ///     Introduces the `EFStackExchangeRedisCacheProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    public static EFCoreSecondLevelCacheOptions UseStackExchangeRedisCacheProvider(
        this EFCoreSecondLevelCacheOptions options,
        string redisConnectionString,
        TimeSpan timeout)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        SetOptions(options, timeout, new EFRedisCacheConfigurationOptions
        {
            RedisConnectionString = redisConnectionString
        });

        return options;
    }

    private static void SetOptions(EFCoreSecondLevelCacheOptions options,
        TimeSpan timeout,
        EFRedisCacheConfigurationOptions configurationOptions)
    {
        options.Settings.Services?.TryAddSingleton<IEFDataSerializer, EFMessagePackSerializer>();
        options.Settings.CacheProvider = typeof(EFStackExchangeRedisCacheProvider);
        options.Settings.AdditionalData = configurationOptions;

        options.Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            Timeout = timeout,
            IsActive = true
        };
    }
}