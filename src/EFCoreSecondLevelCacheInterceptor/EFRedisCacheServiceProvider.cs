using System;
using System.Linq;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Using IRedisDbCache as a cache service.
    /// </summary>
    public class EFRedisCacheServiceProvider : IEFCacheServiceProvider
    {
        private readonly IRedisDbCache _redisDbCache;
        private readonly IReaderWriterLockProvider _readerWriterLockProvider;

        /// <summary>
        /// Using IRedisDbCache as a cache service.
        /// </summary>
        public EFRedisCacheServiceProvider(
            IRedisDbCache redisDbCache,
            IReaderWriterLockProvider readerWriterLockProvider)
        {
            _redisDbCache = redisDbCache;
            _readerWriterLockProvider = readerWriterLockProvider;
        }

        /// <summary>
        /// Removes the cached entries added by this library.
        /// </summary>
        public void ClearAllCachedEntries()
        {
            _readerWriterLockProvider.TryWriteLocked(() => _redisDbCache.Clear());
        }

        /// <summary>
        /// Gets a cached entry by key.
        /// </summary>
        /// <param name="cacheKey">key to find</param>
        /// <returns>cached value</returns>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
        {
            return _readerWriterLockProvider.TryReadLocked(() => _redisDbCache.Get<EFCachedData>(getKey(cacheKey), cachePolicy));
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
                _redisDbCache.Set(getKey(cacheKey), value, cachePolicy);
            });
        }

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
        public void InvalidateCacheDependencies(EFCacheKey cacheKey)
        {
            foreach (var rootCacheKey in cacheKey.CacheDependencies)
            {
                _readerWriterLockProvider.TryWriteLocked(() => _redisDbCache.RemoveByPattern(rootCacheKey));
            }
        }

        private static string getKey(EFCacheKey cacheKey)
        {
            if (cacheKey.CacheDependencies?.Any() != true)
            {
                throw new NullReferenceException(nameof(cacheKey.CacheDependencies));
            }

            if (string.IsNullOrWhiteSpace(cacheKey.KeyHash))
            {
                throw new NullReferenceException(nameof(cacheKey.KeyHash));
            }

            return $"{string.Join("_", cacheKey.CacheDependencies)}_{cacheKey.KeyHash}";
        }
    }
}