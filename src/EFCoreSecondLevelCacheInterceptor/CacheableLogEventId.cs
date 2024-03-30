namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Event IDs of the internal logged messages of the library
/// </summary>
public enum CacheableLogEventId
{
    /// <summary>
    ///     It's not used
    /// </summary>
    None,

    /// <summary>
    ///     The query result is returned from the cache.
    /// </summary>
    CacheHit = CacheableEventId.CacheableBaseId,

    /// <summary>
    ///     The query result is stored in the cache.
    /// </summary>
    QueryResultCached,

    /// <summary>
    ///     The query result was removed from the cache.
    /// </summary>
    QueryResultInvalidated,

    /// <summary>
    ///     The query result was not cached due to some predefined setting.
    /// </summary>
    CachingSkipped,

    /// <summary>
    ///     The query result was not remove from the cached due to some predefined setting.
    /// </summary>
    InvalidationSkipped,

    /// <summary>
    ///     It will be fired when the current interceptor is instantiated for the first time.
    /// </summary>
    CachingSystemStarted,

    /// <summary>
    ///     An exception has been occured
    /// </summary>
    CachingError,

    /// <summary>
    ///     The query result was overwritten by the interceptor for the cache
    /// </summary>
    QueryResultSuppressed,

    /// <summary>
    ///     It will be fired when the cache dependencies if the current query are calculated
    /// </summary>
    CacheDependenciesCalculated,

    /// <summary>
    ///     It will be fired when the cache policy of the current query is calculated
    /// </summary>
    CachePolicyCalculated
}