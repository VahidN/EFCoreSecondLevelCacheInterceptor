using System;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Defines EFCoreSecondLevel's Options
    /// </summary>
    public class EFCoreSecondLevelCacheOptions
    {
        internal EFCoreSecondLevelCacheSettings Settings { get; } = new EFCoreSecondLevelCacheSettings();

        /// <summary>
        /// Puts the whole system in cache. In this case calling the `Cacheable()` methods won't be necessary.
        /// If you specify the `Cacheable()` method, its setting will override this global setting.
        /// If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        public EFCoreSecondLevelCacheOptions CacheAllQueries(CacheExpirationMode expirationMode, TimeSpan timeout)
        {
            Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// Puts the whole system in cache just for the specified `realTableNames`.
        /// In this case calling the `Cacheable()` methods won't be necessary.
        /// If you specify the `Cacheable()` method, its setting will override this global setting.
        /// If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="realTableNames">
        /// The real table names.
        /// Queries containing these names will be cached.
        /// Table names are not case sensitive.
        /// </param>
        public EFCoreSecondLevelCacheOptions CacheQueriesContainingTableNames(
                CacheExpirationMode expirationMode, TimeSpan timeout, params string[] realTableNames)
        {
            Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(entityTypes: null)
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true,
                TableNames = realTableNames
            };
            return this;
        }

        /// <summary>
        /// Puts the whole system in cache just for the specified `entityTypes`.
        /// In this case calling the `Cacheable()` methods won't be necessary.
        /// If you specify the `Cacheable()` method, its setting will override this global setting.
        /// If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="entityTypes">The real entity types. Queries containing these types will be cached.</param>
        public EFCoreSecondLevelCacheOptions CacheQueriesContainingTypes(
                CacheExpirationMode expirationMode, TimeSpan timeout, params Type[] entityTypes)
        {
            Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(entityTypes)
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
        /// </summary>
        /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
        public EFCoreSecondLevelCacheOptions UseCustomCacheProvider<T>() where T : IEFCacheServiceProvider
        {
            Settings.CacheProvider = typeof(T);
            return this;
        }

        /// <summary>
        /// You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
        /// If you specify the `Cacheable()` method options, its setting will override this global setting.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
        public EFCoreSecondLevelCacheOptions UseCustomCacheProvider<T>(CacheExpirationMode expirationMode, TimeSpan timeout) where T : IEFCacheServiceProvider
        {
            Settings.CacheProvider = typeof(T);
            Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
        /// </summary>
        public EFCoreSecondLevelCacheOptions UseMemoryCacheProvider()
        {
            Settings.CacheProvider = typeof(EFMemoryCacheServiceProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
        /// If you specify the `Cacheable()` method options, its setting will override this global setting.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        public EFCoreSecondLevelCacheOptions UseMemoryCacheProvider(CacheExpirationMode expirationMode, TimeSpan timeout)
        {
            Settings.CacheProvider = typeof(EFMemoryCacheServiceProvider);
            Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
        /// </summary>
        public EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider()
        {
            Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);
            return this;
        }

        /// <summary>
        /// Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
        /// If you specify the `Cacheable()` method options, its setting will override this global setting.
        /// </summary>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        public EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider(CacheExpirationMode expirationMode, TimeSpan timeout)
        {
            Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);
            Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
        /// </summary>
        /// <param name="providerName">Selected caching provider name.</param>
        /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
        public EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(string providerName, bool isHybridCache = false)
        {
            Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
            Settings.ProviderName = providerName;
            Settings.IsHybridCache = isHybridCache;
            return this;
        }

        /// <summary>
        /// Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
        /// If you specify the `Cacheable()` method options, its setting will override this global setting.
        /// </summary>
        /// <param name="providerName">Selected caching provider name.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
        public EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(
            string providerName,
            CacheExpirationMode expirationMode,
            TimeSpan timeout,
            bool isHybridCache = false)
        {
            Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
            Settings.ProviderName = providerName;
            Settings.IsHybridCache = isHybridCache;
            Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
            {
                ExpirationMode = expirationMode,
                Timeout = timeout,
                IsActive = true
            };
            return this;
        }

        /// <summary>
        /// Should the debug level loggig be disabled?
        /// Set it to true for maximum performance.
        /// </summary>
        public EFCoreSecondLevelCacheOptions DisableLogging(bool value = false)
        {
            Settings.DisableLogging = value;
            return this;
        }

        /// <summary>
        /// Here you can decide based on the currect executing SQL command, should we cache its result or not?
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public EFCoreSecondLevelCacheOptions SkipCachingCommands(Predicate<string> predicate)
        {
            Settings.SkipCachingCommands = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return this;
        }
    }
}