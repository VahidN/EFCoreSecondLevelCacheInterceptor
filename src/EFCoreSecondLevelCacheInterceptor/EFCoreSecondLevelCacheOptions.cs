using System;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public class EFCoreSecondLevelCacheOptions
{
    internal EFCoreSecondLevelCacheSettings Settings { get; } = new();

    /// <summary>
    ///     Puts the whole system in cache. In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    ///     If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public EFCoreSecondLevelCacheOptions CacheAllQueries(CacheExpirationMode expirationMode, TimeSpan timeout)
    {
        Settings.CacheAllQueriesOptions = new CacheAllQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Puts the whole system in cache just for the specified `realTableNames`.
    ///     In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    ///     If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="tableNameComparison">How should we determine which tables should be cached?</param>
    /// <param name="realTableNames">
    ///     The real table names.
    ///     Queries containing these names will be cached.
    ///     Table names are not case sensitive.
    /// </param>
    public EFCoreSecondLevelCacheOptions CacheQueriesContainingTableNames(
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        TableNameComparison tableNameComparison = TableNameComparison.Contains,
        params string[] realTableNames)
    {
        Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(null)
                                               {
                                                   ExpirationMode = expirationMode,
                                                   Timeout = timeout,
                                                   IsActive = true,
                                                   TableNames = realTableNames,
                                                   TableNameComparison = tableNameComparison,
                                               };
        return this;
    }

    /// <summary>
    ///     Puts the whole system in cache just for the specified `entityTypes`.
    ///     In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    ///     If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="tableTypeComparison">How should we determine which tables should be cached?</param>
    /// <param name="entityTypes">The real entity types. Queries containing these types will be cached.</param>
    public EFCoreSecondLevelCacheOptions CacheQueriesContainingTypes(
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        TableTypeComparison tableTypeComparison = TableTypeComparison.Contains,
        params Type[] entityTypes)
    {
        Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(entityTypes)
                                               {
                                                   ExpirationMode = expirationMode,
                                                   Timeout = timeout,
                                                   IsActive = true,
                                                   TableTypeComparison = tableTypeComparison,
                                               };
        return this;
    }

    /// <summary>
    ///     You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
    /// </summary>
    /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
    public EFCoreSecondLevelCacheOptions UseCustomCacheProvider<T>() where T : IEFCacheServiceProvider
    {
        Settings.CacheProvider = typeof(T);
        return this;
    }

    /// <summary>
    ///     You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
    public EFCoreSecondLevelCacheOptions UseCustomCacheProvider<T>(CacheExpirationMode expirationMode,
                                                                   TimeSpan timeout) where T : IEFCacheServiceProvider
    {
        Settings.CacheProvider = typeof(T);
        Settings.CachableQueriesOptions = new CachableQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
    /// </summary>
    public EFCoreSecondLevelCacheOptions UseMemoryCacheProvider()
    {
        Settings.CacheProvider = typeof(EFMemoryCacheServiceProvider);
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `EFMemoryCacheServiceProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public EFCoreSecondLevelCacheOptions UseMemoryCacheProvider(CacheExpirationMode expirationMode,
                                                                TimeSpan timeout)
    {
        Settings.CacheProvider = typeof(EFMemoryCacheServiceProvider);
        Settings.CachableQueriesOptions = new CachableQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
    /// </summary>
    public EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider()
    {
        Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `CacheManagerCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public EFCoreSecondLevelCacheOptions UseCacheManagerCoreProvider(CacheExpirationMode expirationMode,
                                                                     TimeSpan timeout)
    {
        Settings.CacheProvider = typeof(EFCacheManagerCoreProvider);
        Settings.CachableQueriesOptions = new CachableQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
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
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    /// </summary>
    /// <param name="providerName">
    ///     Selected caching provider name.
    ///     This option will let you to choose a different redis database for your current tenant.
    ///     <![CDATA[ Such as: (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"]; ]]>
    /// </param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(
        Func<IServiceProvider, EFCacheKey?, string> providerName,
        bool isHybridCache = false)
    {
        Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        Settings.CacheProviderName = providerName;
        Settings.IsHybridCache = isHybridCache;
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
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
        Settings.CachableQueriesOptions = new CachableQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Introduces the built-in `EasyCachingCoreProvider` to be used as the CacheProvider.
    ///     If you specify the `Cacheable()` method options, its setting will override this global setting.
    /// </summary>
    /// <param name="providerName">
    ///     Selected caching provider name.
    ///     This option will let you to choose a different redis database for your current tenant.
    ///     <![CDATA[ Such as: (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"]; ]]>
    /// </param>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="isHybridCache">Is an instance of EasyCaching.HybridCache</param>
    public EFCoreSecondLevelCacheOptions UseEasyCachingCoreProvider(
        Func<IServiceProvider, EFCacheKey?, string> providerName,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        bool isHybridCache = false)
    {
        Settings.CacheProvider = typeof(EFEasyCachingCoreProvider);
        Settings.CacheProviderName = providerName;
        Settings.IsHybridCache = isHybridCache;
        Settings.CachableQueriesOptions = new CachableQueriesOptions
                                          {
                                              ExpirationMode = expirationMode,
                                              Timeout = timeout,
                                              IsActive = true,
                                          };
        return this;
    }

    /// <summary>
    ///     Uses the cache key prefix.
    ///     Sets the prefix to all of the cachedKey's.
    ///     Its default value is `EF_`.
    /// </summary>
    /// <param name="prefix">The prefix.</param>
    /// <returns>EFCoreSecondLevelCacheOptions.</returns>
    public EFCoreSecondLevelCacheOptions UseCacheKeyPrefix(string prefix)
    {
        Settings.CacheKeyPrefix = prefix;
        return this;
    }

    /// <summary>
    ///     Should the debug level logging be disabled?
    ///     Set it to true for maximum performance.
    /// </summary>
    public EFCoreSecondLevelCacheOptions DisableLogging(bool value = false)
    {
        Settings.DisableLogging = value;
        return this;
    }

    /// <summary>
    ///     Here you can decide based on the correct executing SQL command, should we cache its result or not?
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="predicate" /> is <c>null</c>.</exception>
    public EFCoreSecondLevelCacheOptions SkipCachingCommands(Predicate<string> predicate)
    {
        Settings.SkipCachingCommands = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    ///     Here you can decide based on the correct executing result, should we cache this result or not?
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="predicate" /> is <c>null</c>.</exception>
    public EFCoreSecondLevelCacheOptions SkipCachingResults(Predicate<(string CommandText, object Value)> predicate)
    {
        Settings.SkipCachingResults = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    ///     Here you can decide based on the correct executing SQL command, should we invalidate the cache or not?
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="predicate" /> is <c>null</c>.</exception>
    public EFCoreSecondLevelCacheOptions SkipCacheInvalidationCommands(Predicate<string> predicate)
    {
        Settings.SkipCacheInvalidationCommands = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    ///     Puts the whole system in cache except for the specified `realTableNames`.
    ///     In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="realTableNames">
    ///     The real table names.
    ///     Queries containing these names will not be cached.
    ///     Table names are not case sensitive.
    /// </param>
    public EFCoreSecondLevelCacheOptions CacheAllQueriesExceptContainingTableNames(
        CacheExpirationMode expirationMode, TimeSpan timeout, params string[] realTableNames)
    {
        Settings.SkipCacheSpecificQueriesOptions = new SkipCacheSpecificQueriesOptions(null)
                                                   {
                                                       ExpirationMode = expirationMode,
                                                       Timeout = timeout,
                                                       IsActive = true,
                                                       TableNames = realTableNames,
                                                   };
        return this;
    }

    /// <summary>
    ///     Puts the whole system in cache except for the specified `entityTypes`.
    ///     In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="entityTypes">The real entity types. Queries containing these types will not be cached.</param>
    public EFCoreSecondLevelCacheOptions CacheAllQueriesExceptContainingTypes(
        CacheExpirationMode expirationMode, TimeSpan timeout, params Type[] entityTypes)
    {
        Settings.SkipCacheSpecificQueriesOptions = new SkipCacheSpecificQueriesOptions(entityTypes)
                                                   {
                                                       ExpirationMode = expirationMode,
                                                       Timeout = timeout,
                                                       IsActive = true,
                                                   };
        return this;
    }
}