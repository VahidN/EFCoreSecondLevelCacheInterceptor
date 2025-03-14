using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace EFCoreSecondLevelCacheInterceptor;

/// <remarks>
///     Using IMemoryCache as a cache service.
/// </remarks>
public class EFMemoryCacheServiceProvider(
    IMemoryCache memoryCache,
    IMemoryCacheChangeTokenProvider signal,
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
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        if (cachePolicy is null)
        {
            throw new ArgumentNullException(nameof(cachePolicy));
        }

        value ??= new EFCachedData
        {
            IsNull = true
        };

        var options = new MemoryCacheEntryOptions
        {
            Size = 1
        };

        switch (cachePolicy.CacheExpirationMode)
        {
            case CacheExpirationMode.NeverRemove:
                // the item will theoretically remain cached indefinitely
                break;
            case CacheExpirationMode.Absolute:
                options.AbsoluteExpirationRelativeToNow = cachePolicy.CacheTimeout;

                break;
            default:
                options.SlidingExpiration = cachePolicy.CacheTimeout;

                break;
        }

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            options.ExpirationTokens.Add(signal.GetChangeToken(rootCacheKey));
        }

        memoryCache.Set(cacheKey.KeyHash, value, options);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries()
    {
        signal.RemoveAllChangeTokens();

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
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        return memoryCache.Get<EFCachedData>(cacheKey.KeyHash);
    }

    /// <summary>
    ///     Invalidates all the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        if (cacheKey == null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            signal.RemoveChangeToken(rootCacheKey);
        }
    }
}