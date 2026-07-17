using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Entity Framework Core Second Level Caching Library
/// </summary>
/// <remarks>
///     Entity Framework Core Second Level Caching Library
///     Please use
///     services.AddDbContextPool&lt;ApplicationDbContext&gt;((serviceProvider, optionsBuilder) =&gt;
///     optionsBuilder.UseSqlServer(...).AddInterceptors(serviceProvider.GetRequiredService&lt;SecondLevelCacheInterceptor
///     &gt;()));
///     to register it.
/// </remarks>
public class SecondLevelCacheInterceptor(IDbCommandInterceptorProcessor processor) : DbCommandInterceptor
{
    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQuery
    /// </summary>
    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        => processor.ProcessExecutedCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQueryAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
#else
    public override Task<int> NonQueryExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutedCommandsAsync =
            processor.ProcessExecutedCommandsAsync(command, eventData?.Context, result, cancellationToken);
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<int>(processExecutedCommandsAsync);
#else
        return processExecutedCommandsAsync;
#endif
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQuery.
    /// </summary>
    public override InterceptionResult<int> NonQueryExecuting(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
        => processor.ProcessExecutingCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQueryAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
#else
    public override Task<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutingCommandsAsync =
            processor.ProcessExecutingCommandsAsync(command, eventData?.Context, result, cancellationToken);
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<InterceptionResult<int>>(processExecutingCommandsAsync);
#else
        return processExecutingCommandsAsync;
#endif
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReader.
    /// </summary>
    public override DbDataReader ReaderExecuted(DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
        => processor.ProcessExecutedCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReaderAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
#else
    public override Task<DbDataReader> ReaderExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutedCommands =
            processor.ProcessExecutedCommandsAsync(command, eventData?.Context, result, cancellationToken);

#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<DbDataReader>(processExecutedCommands);
#else
        return processExecutedCommands;
#endif
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReader.
    /// </summary>
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
        => processor.ProcessExecutingCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReaderAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
#else
    public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutingCommandsAsync =
            processor.ProcessExecutingCommandsAsync(command, eventData?.Context, result, cancellationToken);
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<InterceptionResult<DbDataReader>>(processExecutingCommandsAsync);
#else
        return processExecutingCommandsAsync;
#endif
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalar.
    /// </summary>
    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        => processor.ProcessExecutedCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalarAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
#else
    public override Task<object> ScalarExecutedAsync(DbCommand command,
        CommandExecutedEventData eventData,
        object result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutedCommandsAsync =
            processor.ProcessExecutedCommandsAsync(command, eventData?.Context, result, cancellationToken);

#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<object?>(processExecutedCommandsAsync);
#else
        return processExecutedCommandsAsync;
#endif
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalar.
    /// </summary>
    public override InterceptionResult<object> ScalarExecuting(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
        => processor.ProcessExecutingCommands(command, eventData?.Context, result);

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalarAsync.
    /// </summary>
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
#else
    public override Task<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
#endif
    {
        var processExecutingCommandsAsync =
            processor.ProcessExecutingCommandsAsync(command, eventData?.Context, result, cancellationToken);

#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETSTANDARD2_1
        return new ValueTask<InterceptionResult<object>>(processExecutingCommandsAsync);
#else
        return processExecutingCommandsAsync;
#endif
    }
}