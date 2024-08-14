using System;
#if NET5_0 || NET6_0 || NET7_0 || NET8_0
using System.Text.Json;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Global Cache Settings
/// </summary>
public class EFCoreSecondLevelCacheSettings
{
#if NET5_0 || NET6_0 || NET7_0 || NET8_0
    /// <summary>
    ///     Options to control the serialization behavior
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
#endif

    /// <summary>
    ///     The selected cache provider
    /// </summary>
    public Type? CacheProvider { get; set; }

    /// <summary>
    ///     The selected hash provider
    /// </summary>
    public Type? HashProvider { get; set; }

    /// <summary>
    ///     Selected caching provider name
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    ///     This option will let you to choose a different redis database for your current tenant.
    ///     <![CDATA[ Such as: (serviceProvider, cacheKey) => "redis-db-" + serviceProvider.GetRequiredService<IHttpContextAccesor>().HttpContext.Request.Headers["tenant-id"]; ]]>
    /// </summary>
    public Func<IServiceProvider, EFCacheKey?, string>? CacheProviderName { set; get; }

    /// <summary>
    ///     Is an instance of EasyCaching.HybridCache
    /// </summary>
    public bool IsHybridCache { get; set; }

    /// <summary>
    ///     Gets or sets the cache key prefix.
    ///     Its default value is `EF_`.
    /// </summary>
    /// <value>The cache key prefix.</value>
    public string CacheKeyPrefix { get; set; } = "EF_";

    /// <summary>
    ///     Gets or sets a dynamic cache key prefix.
    /// </summary>
    /// <value>The cache key prefix.</value>
    public Func<IServiceProvider, string>? CacheKeyPrefixSelector { get; set; }

    /// <summary>
    ///     CacheAllQueries Options
    /// </summary>
    public CacheAllQueriesOptions CacheAllQueriesOptions { get; set; } = new();

    /// <summary>
    ///     Cache Specific Queries Options
    /// </summary>
    public CacheSpecificQueriesOptions CacheSpecificQueriesOptions { get; set; } = new(entityTypes: null);

    /// <summary>
    ///     Cachable Queries Options
    /// </summary>
    public CachableQueriesOptions CachableQueriesOptions { get; set; } = new();

    /// <summary>
    ///     Skip Cache Specific Queries Options
    /// </summary>
    public SkipCacheSpecificQueriesOptions SkipCacheSpecificQueriesOptions { get; set; } = new(entityTypes: null);

    /// <summary>
    ///     Should the debug level logging be enabled?
    /// </summary>
    public bool EnableLogging { set; get; }

    /// <summary>
    ///     Fallback on db if the caching provider (redis) is down.
    /// </summary>
    public bool UseDbCallsIfCachingProviderIsDown { set; get; }

    /// <summary>
    ///     Set it to false to disable this caching interceptor.
    ///     Its default value is `true`.
    /// </summary>
    public bool IsCachingInterceptorEnabled { set; get; } = true;

    /// <summary>
    ///     The cache server's availability check interval value.
    /// </summary>
    public TimeSpan NextCacheServerAvailabilityCheck { set; get; } = TimeSpan.FromMinutes(value: 1);

    /// <summary>
    ///     Possibility to allow caching with explicit transactions.
    ///     Its default value is false.
    /// </summary>
    public bool AllowCachingWithExplicitTransactions { set; get; }

    /// <summary>
    ///     Here you can decide based on the correct executing SQL command, should we cache its result or not?
    /// </summary>
    public Predicate<string>? SkipCachingCommands { set; get; }

    /// <summary>
    ///     Here you can decide based on the correct executing SQL command, should we invalidate the cache or not?
    /// </summary>
    public Predicate<string>? SkipCacheInvalidationCommands { set; get; }

    /// <summary>
    ///     Here you can decide based on the correct executing result, should we cache this result or not?
    /// </summary>
    public Predicate<(string CommandText, object Value)>? SkipCachingResults { set; get; }

    /// <summary>
    ///     If you set DisableLogging to false, this delegate will give you the internal caching events of the library.
    /// </summary>
    public Action<EFCacheableLogEvent>? CacheableEvent { set; get; }

    /// <summary>
    ///     Determines which entities are involved in the current cache-invalidation event.
    /// </summary>
    public Action<EFCacheInvalidationInfo>? CacheInvalidationEvent { set; get; }
}