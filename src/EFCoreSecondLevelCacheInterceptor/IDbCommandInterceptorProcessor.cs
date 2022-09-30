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
    T ProcessExecutedCommands<T>(DbCommand command, DbContext? context, T result);

    /// <summary>
    ///     Adds command's data to the cache
    /// </summary>
    T ProcessExecutingCommands<T>(DbCommand command, DbContext? context, T result);
}