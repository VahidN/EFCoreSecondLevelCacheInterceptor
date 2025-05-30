using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Helps process SecondLevelCacheInterceptor
/// </summary>
public interface IDbCommandIgnoreCachingProcessor
{
    /// <summary>
    ///     Is this command marked for caching?
    /// </summary>
    (bool ShouldSkipProcessing, EFCachePolicy? CachePolicy) ShouldSkipProcessing(DbCommand? command,
        DbContext? context,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Skip caching of this result based on the provided predicate
    /// </summary>
    bool ShouldSkipCachingResults(string commandText, object value);
}