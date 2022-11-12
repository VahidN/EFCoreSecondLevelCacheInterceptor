using System;
using System.Collections.Generic;
using EasyCaching.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Using ICacheManager as a cache service.
/// </summary>
public class EFEasyCachingCoreProvider : IEFCacheServiceProvider
{
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

    private readonly IEasyCachingProviderBase _easyCachingProvider;

    private readonly IEFDebugLogger _logger;

    /// <summary>
    ///     Using IMemoryCache as a cache service.
    /// </summary>
    public EFEasyCachingCoreProvider(
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IServiceProvider serviceProvider,
        IEFDebugLogger logger)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _cacheSettings = cacheSettings.Value;
        _logger = logger;

        if (_cacheSettings.IsHybridCache)
        {
            var hybridFactory = serviceProvider.GetRequiredService<IHybridProviderFactory>();
            _easyCachingProvider = hybridFactory.GetHybridCachingProvider(_cacheSettings.ProviderName);
        }
        else
        {
            var providerFactory = serviceProvider.GetRequiredService<IEasyCachingProviderFactory>();
            _easyCachingProvider = providerFactory.GetCachingProvider(_cacheSettings.ProviderName);
        }
    }

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

        if (value == null)
        {
            value = new EFCachedData { IsNull = true };
        }

        var keyHash = cacheKey.KeyHash;

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }

            var items = _easyCachingProvider.Get<HashSet<string>>(rootCacheKey);
            if (items.IsNull)
            {
                _easyCachingProvider.Set(rootCacheKey,
                                         new HashSet<string>(StringComparer.OrdinalIgnoreCase) { keyHash },
                                         cachePolicy.CacheTimeout);
            }
            else
            {
                items.Value.Add(keyHash);
                _easyCachingProvider.Set(rootCacheKey, items.Value, cachePolicy.CacheTimeout);
            }
        }

        // We don't support Sliding Expiration at this time. -> https://github.com/dotnetcore/EasyCaching/issues/113
        _easyCachingProvider.Set(keyHash, value, cachePolicy.CacheTimeout);
    }

    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    public void ClearAllCachedEntries()
    {
        if (!_cacheSettings.IsHybridCache)
        {
            ((IEasyCachingProvider)_easyCachingProvider).Flush();
        }
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

        return _easyCachingProvider.Get<EFCachedData>(cacheKey.KeyHash).Value;
    }

    /// <summary>
    ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }

            var cachedValue = _easyCachingProvider.Get<EFCachedData>(cacheKey.KeyHash);
            var dependencyKeys = _easyCachingProvider.Get<HashSet<string>>(rootCacheKey);
            if (AreRootCacheKeysExpired(cachedValue, dependencyKeys))
            {
                _logger.LogDebug(CacheableEventId.QueryResultInvalidated,
                                 $"Invalidated all of the cache entries due to early expiration of a root cache key[{rootCacheKey}].");
                ClearAllCachedEntries();
                return;
            }

            ClearDependencyValues(dependencyKeys);
            _easyCachingProvider.Remove(rootCacheKey);
        }
    }

    private void ClearDependencyValues(CacheValue<HashSet<string>> dependencyKeys)
    {
        if (dependencyKeys.IsNull)
        {
            return;
        }

        foreach (var dependencyKey in dependencyKeys.Value)
        {
            _easyCachingProvider.Remove(dependencyKey);
        }
    }

    private static bool AreRootCacheKeysExpired(
        CacheValue<EFCachedData> cachedValue, CacheValue<HashSet<string>> dependencyKeys)
        => !cachedValue.IsNull && dependencyKeys.IsNull;
}