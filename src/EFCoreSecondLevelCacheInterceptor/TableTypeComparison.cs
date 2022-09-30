namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     How should we determine which tables should be cached?
/// </summary>
public enum TableTypeComparison
{
    /// <summary>
    ///     Caches queries containing table types having the specified types.
    /// </summary>
    Contains,

    /// <summary>
    ///     Caches queries containing table types not having the specified types.
    /// </summary>
    DoesNotContain,

    /// <summary>
    ///     Caches queries containing table types equal to the specified types exclusively.
    /// </summary>
    ContainsOnly,

    /// <summary>
    ///     Caches queries containing table types equal to every one of the specified types exclusively.
    /// </summary>
    ContainsEvery,

    /// <summary>
    ///     Caches queries containing table types not equal to every one of the specified types exclusively.
    /// </summary>
    DoesNotContainEvery,
}