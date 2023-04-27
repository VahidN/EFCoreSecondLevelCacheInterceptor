using System;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     A custom cache key prefix provider for EF queries.
/// </summary>
public class EFCacheKeyPrefixProvider : IEFCacheKeyPrefixProvider
{
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     A custom cache key prefix provider for EF queries.
    /// </summary>
    public EFCacheKeyPrefixProvider(IServiceProvider serviceProvider,
                                    IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
    {
        _serviceProvider = serviceProvider;
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheSettings = cacheSettings.Value;
    }

    /// <summary>
    ///     returns the current provided cache key prefix
    /// </summary>
    public string GetCacheKeyPrefix() => _cacheSettings.CacheKeyPrefixSelector is not null
                                             ? _cacheSettings.CacheKeyPrefixSelector(_serviceProvider)
                                             : _cacheSettings.CacheKeyPrefix;
}