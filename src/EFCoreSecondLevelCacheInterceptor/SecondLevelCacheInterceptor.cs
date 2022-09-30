using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Entity Framework Core Second Level Caching Library
/// </summary>
public class SecondLevelCacheInterceptor : DbCommandInterceptor
{
    private readonly ILockProvider _lockProvider;
    private readonly IDbCommandInterceptorProcessor _processor;
    private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;

    /// <summary>
    ///     Entity Framework Core Second Level Caching Library
    ///     Please use
    ///     services.AddDbContextPool&lt;ApplicationDbContext&gt;((serviceProvider, optionsBuilder) =&gt;
    ///     optionsBuilder.UseSqlServer(...).AddInterceptors(serviceProvider.GetRequiredService&lt;SecondLevelCacheInterceptor
    ///     &gt;()));
    ///     to register it.
    /// </summary>
    public SecondLevelCacheInterceptor(
        IDbCommandInterceptorProcessor processor,
        IEFSqlCommandsProcessor sqlCommandsProcessor,
        ILockProvider lockProvider)
    {
        _processor = processor;
        _sqlCommandsProcessor = sqlCommandsProcessor;
        _lockProvider = lockProvider;
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQuery
    /// </summary>
    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    private bool ShouldSkipProcessing(DbCommand? command, DbContextEventData? eventData)
    {
        if (eventData?.Context is null)
        {
            return true;
        }

        if (command?.Transaction is not null)
        {
            return !_sqlCommandsProcessor.IsCrudCommand(command.CommandText);
        }

        return false;
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQueryAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQuery.
    /// </summary>
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQueryAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReader.
    /// </summary>
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReaderAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReader.
    /// </summary>
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReaderAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalar.
    /// </summary>
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalarAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<object> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutedCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalar.
    /// </summary>
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        using var @lock = _lockProvider.Lock();
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }

    /// <summary>
    ///     Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalarAsync.
    /// </summary>
#if NET6_0 || NET5_0 || NETSTANDARD2_1
    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
#else
        public override async Task<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
#endif
    {
        using var lockAsync = await _lockProvider.LockAsync(cancellationToken);
        return ShouldSkipProcessing(command, eventData)
                   ? result
                   : _processor.ProcessExecutingCommands(command, eventData.Context, result);
    }
}