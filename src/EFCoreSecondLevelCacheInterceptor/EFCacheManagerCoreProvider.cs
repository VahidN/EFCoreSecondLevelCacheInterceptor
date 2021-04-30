using System;
using System.Collections.Generic;
using CacheManager.Core;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Using ICacheManager as a cache service.
    /// </summary>
    public class EFCacheManagerCoreProvider : IEFCacheServiceProvider
    {
        private readonly IReaderWriterLockProvider _readerWriterLockProvider;
        private readonly ICacheManager<ISet<string>> _dependenciesCacheManager;
        private readonly ICacheManager<EFCachedData> _valuesCacheManager;
        private readonly IEFDebugLogger _logger;

        /// <summary>
        /// Using IMemoryCache as a cache service.
        /// </summary>
        public EFCacheManagerCoreProvider(
            ICacheManager<ISet<string>> dependenciesCacheManager,
            ICacheManager<EFCachedData> valuesCacheManager,
            IReaderWriterLockProvider readerWriterLockProvider,
            IEFDebugLogger logger)
        {
            _readerWriterLockProvider = readerWriterLockProvider;
            _dependenciesCacheManager = dependenciesCacheManager ?? throw new ArgumentNullException(nameof(dependenciesCacheManager), "Please register the `ICacheManager`.");
            _valuesCacheManager = valuesCacheManager ?? throw new ArgumentNullException(nameof(valuesCacheManager), "Please register the `ICacheManager`.");
            _logger = logger;

            // Occurs when an item was removed by the cache handle due to expiration or e.g. memory pressure eviction.
            // Without _dependenciesCacheManager items, we can't invalidate cached items on Insert/Update/Delete.
            // So to prevent stale reads, we have to clear all cached data in this case.
            _dependenciesCacheManager.OnRemoveByHandle += (sender, args) => ClearAllCachedEntries();
        }

        /// <summary>
        /// Adds a new item to the cache.
        /// </summary>
        /// <param name="cacheKey">key</param>
        /// <param name="value">value</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
                if (value == null)
                {
                    value = new EFCachedData { IsNull = true };
                }

                var keyHash = cacheKey.KeyHash;

                foreach (var rootCacheKey in cacheKey.CacheDependencies)
                {
                    _dependenciesCacheManager.AddOrUpdate(
                        rootCacheKey,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { keyHash },
                        updateValue: set =>
                        {
                            set.Add(keyHash);
                            return set;
                        });
                }

                if (cachePolicy == null)
                {
                    _valuesCacheManager.Add(keyHash, value);
                }
                else
                {
                    _valuesCacheManager.Add(new CacheItem<EFCachedData>(
                        keyHash,
                        value,
                        cachePolicy.CacheExpirationMode == CacheExpirationMode.Absolute ? ExpirationMode.Absolute : ExpirationMode.Sliding,
                        cachePolicy.CacheTimeout));
                }
            });
        }

        /// <summary>
        /// Removes the cached entries added by this library.
        /// </summary>
        public void ClearAllCachedEntries()
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
                _valuesCacheManager.Clear();
                _dependenciesCacheManager.Clear();
            });
        }

        /// <summary>
        /// Gets a cached entry by key.
        /// </summary>
        /// <param name="cacheKey">key to find</param>
        /// <returns>cached value</returns>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
        {
            return _readerWriterLockProvider.TryReadLocked(() => _valuesCacheManager.Get<EFCachedData>(cacheKey.KeyHash));
        }

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
        public void InvalidateCacheDependencies(EFCacheKey cacheKey)
        {
            _readerWriterLockProvider.TryWriteLocked(() =>
            {
                foreach (var rootCacheKey in cacheKey.CacheDependencies)
                {
                    if (string.IsNullOrWhiteSpace(rootCacheKey))
                    {
                        continue;
                    }

                    var cachedValue = _valuesCacheManager.Get<EFCachedData>(cacheKey.KeyHash);
                    var dependencyKeys = _dependenciesCacheManager.Get<HashSet<string>>(rootCacheKey);
                    if (areRootCacheKeysExpired(cachedValue, dependencyKeys))
                    {
                        _logger.LogDebug(CacheableEventId.QueryResultInvalidated,
                            $"Invalidated all of the cache entries due to early expiration of a root cache key[{rootCacheKey}].");
                        ClearAllCachedEntries();
                        return;
                    }

                    clearDependencyValues(dependencyKeys);
                    _dependenciesCacheManager.Remove(rootCacheKey);
                }
            });
        }

        private void clearDependencyValues(HashSet<string>? dependencyKeys)
        {
            if (dependencyKeys is null)
            {
                return;
            }

            foreach (var dependencyKey in dependencyKeys)
            {
                _valuesCacheManager.Remove(dependencyKey);
            }
        }

        private static bool areRootCacheKeysExpired(
                EFCachedData? cachedValue,
                HashSet<string>? dependencyKeys)
            => cachedValue is not null && dependencyKeys is null;
    }
}