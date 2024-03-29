using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Event IDs for events that correspond to messages logged to an ILogger
/// </summary>
public static class CacheableEventId
{
    /// <summary>
    ///     The lower-bound for event IDs used by any Entity Framework or provider code.
    /// </summary>
    public const int CacheableBaseId = 100_000;

    private static readonly string _queryPrefix = $"{DbLoggerCategory.Query.Name}.";

    /// <summary>
    ///     A query result is returned from cache.
    /// </summary>
    public static readonly EventId CacheHit = MakeQueryId(Id.CacheHit);

    /// <summary>
    ///     A query result is stored by the cache.
    /// </summary>
    public static readonly EventId QueryResultCached = MakeQueryId(Id.QueryResultCached);

    /// <summary>
    ///     A query result is removed from the cache.
    /// </summary>
    public static readonly EventId QueryResultInvalidated = MakeQueryId(Id.QueryResultInvalidated);

    private static EventId MakeQueryId(Id id) => new((int)id, _queryPrefix + id);

    private enum Id
    {
        CacheHit = CacheableBaseId,
        QueryResultCached,
        QueryResultInvalidated,
    }
}