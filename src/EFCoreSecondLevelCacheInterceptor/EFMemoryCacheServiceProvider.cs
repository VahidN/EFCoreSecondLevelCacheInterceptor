using System;
using Microsoft.Extensions.Caching.Memory;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Using IMemoryCache as a cache service.
/// </summary>
/// <remarks>
///     Using IMemoryCache as a cache service.
/// </remarks>
public class EFMemoryCacheServiceProvider(IMemoryCache memoryCache, IMemoryCacheChangeTokenProvider signal)
    : IEFCacheServiceProvider
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IMemoryCacheChangeTokenProvider _signal = signal;

    /// <summary>
    ///     Adds a new item to the cache.
    /// </summary>
    /// <param name="cacheKey">key</param>
    /// <param name="value">value</param>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
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

        if (cachePolicy.CacheExpirationMode == CacheExpirationMode.Absolute)
        {
            options.AbsoluteExpirationRelativeToNow = cachePolicy.CacheTimeout;
        }
        else
        {
            options.SlidingExpiration = cachePolicy.CacheTimeout;
        }

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            options.ExpirationTokens.Add(_signal.GetChangeToken(rootCacheKey));
        }

        _memoryCache.Set(cacheKey.KeyHash, value, options);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries() => _signal.RemoveAllChangeTokens();

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

        return _memoryCache.Get<EFCachedData>(cacheKey.KeyHash);
    }

    /// <summary>
    ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
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
            _signal.RemoveChangeToken(rootCacheKey);
        }
    }
}