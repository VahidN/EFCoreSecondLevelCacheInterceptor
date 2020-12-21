using System;
using System.Collections.Generic;
using EasyCaching.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Using ICacheManager as a cache service.
    /// </summary>
    public class EFEasyCachingCoreProvider : IEFCacheServiceProvider
    {
        private readonly IReaderWriterLockProvider _readerWriterLockProvider;

        private readonly IEasyCachingProviderBase _easyCachingProvider;
        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

        /// <summary>
        /// Using IMemoryCache as a cache service.
        /// </summary>
        public EFEasyCachingCoreProvider(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            IServiceProvider serviceProvider,
            IReaderWriterLockProvider readerWriterLockProvider)
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
            _readerWriterLockProvider = readerWriterLockProvider ?? throw new ArgumentNullException(nameof(readerWriterLockProvider));

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
                    var items = _easyCachingProvider.Get<HashSet<string>>(rootCacheKey);
                    if (items.IsNull)
                    {
                        _easyCachingProvider.Set(rootCacheKey, new HashSet<string> { keyHash }, cachePolicy.CacheTimeout);
                    }
                    else
                    {
                        items.Value.Add(keyHash);
                        _easyCachingProvider.Set(rootCacheKey, items.Value, cachePolicy.CacheTimeout);
                    }
                }

                // We don't support Sliding Expiration at this time. -> https://github.com/dotnetcore/EasyCaching/issues/113
                _easyCachingProvider.Set(keyHash, value, cachePolicy.CacheTimeout);
            });
        }

        /// <summary>
        /// Removes the cached entries added by this library.
        /// </summary>
        public void ClearAllCachedEntries()
        {
            if (!_cacheSettings.IsHybridCache)
            {
                _readerWriterLockProvider.TryWriteLocked(() => ((IEasyCachingProvider)_easyCachingProvider).Flush());
            }
        }

        /// <summary>
        /// Gets a cached entry by key.
        /// </summary>
        /// <param name="cacheKey">key to find</param>
        /// <returns>cached value</returns>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
        {
            return _readerWriterLockProvider.TryReadLocked(() => _easyCachingProvider.Get<EFCachedData>(cacheKey.KeyHash).Value);
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

                    clearDependencyValues(rootCacheKey);
                    _easyCachingProvider.Remove(rootCacheKey);
                }
            });
        }

        private void clearDependencyValues(string rootCacheKey)
        {
            var dependencyKeys = _easyCachingProvider.Get<HashSet<string>>(rootCacheKey);
            if (dependencyKeys.IsNull)
            {
                return;
            }

            foreach (var dependencyKey in dependencyKeys.Value)
            {
                _easyCachingProvider.Remove(dependencyKey);
            }
        }
    }
}