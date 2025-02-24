using System;
#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
using System.Text.Json;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Defines EFCoreSecondLevel's Options
/// </summary>
public class EFCoreSecondLevelCacheOptions
{
    /// <summary>
    ///     Global Cache Settings
    /// </summary>
    public EFCoreSecondLevelCacheSettings Settings { get; } = new();

#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
    /// <summary>
    ///     Provides options to control the serialization behavior.
    ///     EFCacheKeyProvider uses these options to serialize the parameter values.
    ///     Its default value is null.
    /// </summary>
    public EFCoreSecondLevelCacheOptions UseJsonSerializerOptions(JsonSerializerOptions? options)
    {
        Settings.JsonSerializerOptions = options;

        return this;
    }
#endif

    /// <summary>
    ///     Puts the whole system in cache. In this case calling the `Cacheable()` methods won't be necessary.
    ///     If you specify the `Cacheable()` method, its setting will override this global setting.
    ///     If you want to exclude some queries from this global cache, apply the `NotCacheable()` method to them.
    /// </summary>
    /// <param name="expirationMode">Defines the expiration mode of the cache items globally.</param>
    /// <param name="timeout">The expiration timeout.</param>
    public EFCoreSecondLevelCacheOptions CacheAllQueries(CacheExpirationMode expirationMode, TimeSpan? timeout = null)
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
    ///     Table names are not case-sensitive.
    /// </param>
    public EFCoreSecondLevelCacheOptions CacheQueriesContainingTableNames(CacheExpirationMode expirationMode,
        TimeSpan? timeout = null,
        TableNameComparison tableNameComparison = TableNameComparison.Contains,
        params string[] realTableNames)
    {
        Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(entityTypes: null)
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true,
            TableNames = realTableNames,
            TableNameComparison = tableNameComparison
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
    public EFCoreSecondLevelCacheOptions CacheQueriesContainingTypes(CacheExpirationMode expirationMode,
        TimeSpan? timeout = null,
        TableTypeComparison tableTypeComparison = TableTypeComparison.Contains,
        params Type[] entityTypes)
    {
        Settings.CacheSpecificQueriesOptions = new CacheSpecificQueriesOptions(entityTypes)
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true,
            TableTypeComparison = tableTypeComparison
        };

        return this;
    }

    /// <summary>
    ///     You can introduce a custom IEFHashProvider to be used as the HashProvider.
    ///     If you don't specify a custom hash provider, the default `XxHash64Unsafe` provider will be used.
    ///     `xxHash` is an extremely fast `non-cryptographic` Hash algorithm, working at speeds close to RAM limits.
    /// </summary>
    /// <typeparam name="T">Implements IEFHashProvider</typeparam>
    public EFCoreSecondLevelCacheOptions UseCustomHashProvider<T>()
        where T : IEFHashProvider
    {
        Settings.HashProvider = typeof(T);

        return this;
    }

    /// <summary>
    ///     You can introduce a custom IEFCacheServiceProvider to be used as the CacheProvider.
    /// </summary>
    /// <typeparam name="T">Implements IEFCacheServiceProvider</typeparam>
    public EFCoreSecondLevelCacheOptions UseCustomCacheProvider<T>()
        where T : IEFCacheServiceProvider
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
        TimeSpan? timeout = null)
        where T : IEFCacheServiceProvider
    {
        Settings.CacheProvider = typeof(T);

        Settings.CachableQueriesOptions = new CachableQueriesOptions
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return this;
    }

    /// <summary>
    ///     Sets a dynamic prefix for the current cachedKey.
    /// </summary>
    /// <param name="prefix">
    ///     Selected cache key prefix.
    ///     This option will let you choose a different cache key prefix for your current tenant.
    ///     <![CDATA[ Such as: serviceProvider => "EF_" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"] ]]>
    /// </param>
    /// <returns>EFCoreSecondLevelCacheOptions.</returns>
    public EFCoreSecondLevelCacheOptions UseCacheKeyPrefix(Func<IServiceProvider, string>? prefix)
    {
        Settings.CacheKeyPrefixSelector = prefix;

        return this;
    }

    /// <summary>
    ///     Uses the cache key prefix.
    ///     Sets the prefix to all the cachedKey's.
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
    ///     Should the debug level logging be enabled?
    ///     Set it to false for maximum performance.
    /// </summary>
    /// <param name="enable">Set it to true, to enable logging</param>
    /// <param name="cacheableEvent">
    ///     If you set EnableLogging to true, this delegate will give you the internal caching
    ///     events of the library.
    /// </param>
    /// <returns></returns>
    public EFCoreSecondLevelCacheOptions ConfigureLogging(bool enable = false,
        Action<EFCacheableLogEvent>? cacheableEvent = null)
    {
        Settings.EnableLogging = enable;
        Settings.CacheableEvent = cacheableEvent;

        return this;
    }

    /// <summary>
    ///     Determines which entities are involved in the current cache-invalidation event.
    /// </summary>
    public EFCoreSecondLevelCacheOptions NotifyCacheInvalidation(Action<EFCacheInvalidationInfo>? cacheableEvent = null)
    {
        Settings.CacheInvalidationEvent = cacheableEvent;

        return this;
    }

    /// <summary>
    ///     Fallback on db if the caching provider (redis) is down.
    /// </summary>
    public EFCoreSecondLevelCacheOptions UseDbCallsIfCachingProviderIsDown(TimeSpan nextAvailabilityCheck)
    {
        Settings.UseDbCallsIfCachingProviderIsDown = true;
        Settings.NextCacheServerAvailabilityCheck = nextAvailabilityCheck;

        return this;
    }

    /// <summary>
    ///     Set it to `false` to disable this caching interceptor entirely.
    ///     Its default value is `true`.
    /// </summary>
    public EFCoreSecondLevelCacheOptions EnableCachingInterceptor(bool enable = true)
    {
        Settings.IsCachingInterceptorEnabled = enable;

        return this;
    }

    /// <summary>
    ///     Possibility to allow caching with explicit transactions.
    ///     Its default value is false.
    /// </summary>
    public EFCoreSecondLevelCacheOptions AllowCachingWithExplicitTransactions(bool value = false)
    {
        Settings.AllowCachingWithExplicitTransactions = value;

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
    ///     Here you can decide based on the given context, should we cache its result or not?
    /// </summary>
    /// <param name="dbContextTypes">The real DbContext types. Queries containing these types will not be cached.</param>
    public EFCoreSecondLevelCacheOptions SkipCachingDbContexts(params Type[]? dbContextTypes)
    {
        Settings.SkipCachingDbContexts = dbContextTypes;

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
    ///     Table names are not case-sensitive.
    /// </param>
    public EFCoreSecondLevelCacheOptions CacheAllQueriesExceptContainingTableNames(CacheExpirationMode expirationMode,
        TimeSpan? timeout = null,
        params string[] realTableNames)
    {
        Settings.SkipCacheSpecificQueriesOptions = new SkipCacheSpecificQueriesOptions(entityTypes: null)
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true,
            TableNames = realTableNames
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
    public EFCoreSecondLevelCacheOptions CacheAllQueriesExceptContainingTypes(CacheExpirationMode expirationMode,
        TimeSpan? timeout = null,
        params Type[] entityTypes)
    {
        Settings.SkipCacheSpecificQueriesOptions = new SkipCacheSpecificQueriesOptions(entityTypes)
        {
            ExpirationMode = expirationMode,
            Timeout = timeout,
            IsActive = true
        };

        return this;
    }

    /// <summary>
    ///     Here you can override the default/calculated cache-policy of the current query.
    ///     Return null, if you want to use the default/calculated settings.
    /// </summary>
    public EFCoreSecondLevelCacheOptions OverrideCachePolicy(Func<EFCachePolicyContext, EFCachePolicy?> cachePolicy)
    {
        Settings.OverrideCachePolicy = cachePolicy ?? throw new ArgumentNullException(nameof(cachePolicy));

        return this;
    }
}