using System;
using System.Collections.Generic;
using ZiggyCreatures.Caching.Fusion;

namespace EFCoreSecondLevelCacheInterceptor;

/// <remarks>
///     Using FusionCache as a cache service.
/// </remarks>
public class EFFusionCacheProvider(IFusionCache fusionCache, IEFDebugLogger logger) : IEFCacheServiceProvider
{
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

        fusionCache.Set(cacheKey.KeyHash, value, entryOptions =>
        {
            entryOptions.SetDuration(cachePolicy.CacheTimeout);

            if (cachePolicy.CacheExpirationMode == CacheExpirationMode.Sliding)
            {
                entryOptions.SetFailSafe(isEnabled: true, cachePolicy.CacheTimeout.Add(cachePolicy.CacheTimeout));
            }
        }, cacheKey.CacheDependencies);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries()
    {
        fusionCache.Clear();

        logger.NotifyCacheInvalidation(clearAllCachedEntries: true,
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

        return fusionCache.GetOrDefault<EFCachedData>(cacheKey.KeyHash);
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
            fusionCache.RemoveByTag(rootCacheKey);
        }
    }
}