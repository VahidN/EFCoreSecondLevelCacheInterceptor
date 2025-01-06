using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace EFCoreSecondLevelCacheInterceptor;

/// <remarks>
///     Using MS.HybridCache as a cache service.
/// </remarks>
public class EFHybridCacheProvider(
    HybridCache hybridCache,
    IEFHybridCacheDependenciesStore cacheDependenciesStore,
    IEFDebugLogger logger) : IEFCacheServiceProvider
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

        // NOTE: hybridCache doesn't have a synchronous API!! So ... we will wait!

        value ??= new EFCachedData
        {
            IsNull = true
        };

        hybridCache.SetAsync(cacheKey.KeyHash, value, new HybridCacheEntryOptions
        {
            Expiration = cachePolicy.CacheTimeout,
            LocalCacheExpiration = cachePolicy.CacheTimeout
        }, cacheKey.CacheDependencies);

        cacheDependenciesStore.AddCacheDependencies(cacheKey.CacheDependencies);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries()
    {
        InvalidateTaggedEntries(cacheDependenciesStore.GetAllCacheDependencies());

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

        return hybridCache.GetOrCreateAsync<EFCachedData?>(cacheKey.KeyHash, factory
            => ValueTask.FromResult<EFCachedData?>(new EFCachedData
            {
                IsNull = true
            }));
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
            hybridCache.RemoveByTagAsync(rootCacheKey);
        }

        cacheDependenciesStore.RemoveCacheDependencies(cacheDependencies);
    }
}