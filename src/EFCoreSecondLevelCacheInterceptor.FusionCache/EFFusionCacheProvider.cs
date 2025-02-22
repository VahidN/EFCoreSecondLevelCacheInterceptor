using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace EFCoreSecondLevelCacheInterceptor;

/// <remarks>
///     Using FusionCache as a cache service.
/// </remarks>
public class EFFusionCacheProvider : IEFCacheServiceProvider
{
    private readonly IFusionCache _fusionCache;
    private readonly IEFDebugLogger _logger;

    /// <remarks>
    ///     Using FusionCache as a cache service.
    /// </remarks>
    public EFFusionCacheProvider(IFusionCacheProvider fusionCacheProvider,
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IEFDebugLogger logger)
    {
        ArgumentNullException.ThrowIfNull(fusionCacheProvider);
        ArgumentNullException.ThrowIfNull(cacheSettings);

        var options = cacheSettings.Value.AdditionalData as EFFusionCacheConfigurationOptions;

        _fusionCache = options?.NamedCache is null
            ? fusionCacheProvider.GetDefaultCache()
            : fusionCacheProvider.GetCache(options.NamedCache);

        _logger = logger;
    }

    /// <summary>
    ///     Adds a new item to the cache.
    /// </summary>
    /// <param name="cacheKey">key</param>
    /// <param name="value">value</param>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    public void InsertValue(EFCacheKey cacheKey, EFCachedData? value, EFCachePolicy cachePolicy)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);
        ArgumentNullException.ThrowIfNull(cachePolicy);

        value ??= new EFCachedData
        {
            IsNull = true
        };

        _fusionCache.Set(cacheKey.KeyHash, value, entryOptions =>
        {
            if (cachePolicy.CacheExpirationMode != CacheExpirationMode.NeverRemove && cachePolicy.CacheTimeout.HasValue)
            {
                entryOptions.SetDuration(cachePolicy.CacheTimeout.Value);

                if (cachePolicy.CacheExpirationMode == CacheExpirationMode.Sliding)
                {
                    entryOptions.SetFailSafe(isEnabled: true,
                        cachePolicy.CacheTimeout.Value.Add(cachePolicy.CacheTimeout.Value));
                }
            }
        }, cacheKey.CacheDependencies);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries()
    {
        _fusionCache.Clear();

        _logger.NotifyCacheInvalidation(clearAllCachedEntries: true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Gets a cached entry by key.
    /// </summary>
    /// <param name="cacheKey">key to find</param>
    /// <returns>cached value</returns>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    public EFCachedData? GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);

        return _fusionCache.GetOrDefault<EFCachedData>(cacheKey.KeyHash);
    }

    /// <summary>
    ///     Invalidates all the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        ArgumentNullException.ThrowIfNull(cacheKey);

        InvalidateTaggedEntries(cacheKey.CacheDependencies);
    }

    private void InvalidateTaggedEntries(ISet<string> cacheDependencies)
    {
        foreach (var rootCacheKey in cacheDependencies)
        {
            _fusionCache.RemoveByTag(rootCacheKey);
        }
    }
}