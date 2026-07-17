using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Helps process SecondLevelCacheInterceptor
/// </summary>
public interface IDbCommandInterceptorProcessor
{
    /// <summary>
    ///     Reads data from cache or cache it and then returns the result
    /// </summary>
    T ProcessExecutedCommands<T>(DbCommand command, DbContext? context, T result);

    /// <summary>
    ///     Reads data from cache or cache it and then returns the result
    /// </summary>
    Task<T> ProcessExecutedCommandsAsync<T>(DbCommand command,
        DbContext? context,
        T result,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Adds command's data to the cache
    /// </summary>
    T ProcessExecutingCommands<T>(DbCommand command, DbContext? context, T result);

    /// <summary>
    ///     Adds command's data to the cache
    /// </summary>
    Task<T> ProcessExecutingCommandsAsync<T>(DbCommand command,
        DbContext? context,
        T result,
        CancellationToken cancellationToken);
}