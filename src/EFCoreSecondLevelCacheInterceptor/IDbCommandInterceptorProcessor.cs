using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Helps processing SecondLevelCacheInterceptor
/// </summary>
public interface IDbCommandInterceptorProcessor
{
    /// <summary>
    ///     Reads data from cache or cache it and then returns the result
    /// </summary>
    T ProcessExecutedCommands<T>(DbCommand command, DbContext? context, T result, EFCachePolicy? cachePolicy);

    /// <summary>
    ///     Adds command's data to the cache
    /// </summary>
    T ProcessExecutingCommands<T>(DbCommand command, DbContext? context, T result, EFCachePolicy? cachePolicy);

    /// <summary>
    ///     Is this command marked for caching?
    /// </summary>
    (bool ShouldSkipProcessing, EFCachePolicy? CachePolicy)
        ShouldSkipProcessing(DbCommand? command, DbContext? context);
}