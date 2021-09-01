using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Entity Framework Core Second Level Caching Library
    /// </summary>
    public class SecondLevelCacheInterceptor : DbCommandInterceptor
    {
        private readonly IDbCommandInterceptorProcessor _processor;
        private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;

        /// <summary>
        /// Entity Framework Core Second Level Caching Library
        /// Please use
        /// services.AddDbContextPool&lt;ApplicationDbContext&gt;((serviceProvider, optionsBuilder) =&gt;
        ///                   optionsBuilder.UseSqlServer(...).AddInterceptors(serviceProvider.GetRequiredService&lt;SecondLevelCacheInterceptor&gt;()));
        /// to register it.
        /// </summary>
        public SecondLevelCacheInterceptor(
            IDbCommandInterceptorProcessor processor,
            IEFSqlCommandsProcessor sqlCommandsProcessor)
        {
            _processor = processor;
            _sqlCommandsProcessor = sqlCommandsProcessor;
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQuery
        /// </summary>
        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutedCommands(command, eventData.Context, result);
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
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQueryAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
#else
        public override Task<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<int>(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#endif
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQuery.
        /// </summary>
        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQueryAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
#else
        public override Task<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<InterceptionResult<int>>(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#endif
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReader.
        /// </summary>
        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutedCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReaderAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
#else
        public override Task<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<DbDataReader>(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#endif
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReader.
        /// </summary>
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReaderAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
#else
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<InterceptionResult<DbDataReader>>(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#endif
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalar.
        /// </summary>
        public override object? ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutedCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalarAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
#else
        public override Task<object> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<object?>(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutedCommands(command, eventData.Context, result));
#endif
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalar.
        /// </summary>
        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            if (ShouldSkipProcessing(command, eventData))
            {
                return result;
            }

            return _processor.ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalarAsync.
        /// </summary>
#if NET5_0 || NETSTANDARD2_1
        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
#else
        public override Task<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
#endif
        {
            if (ShouldSkipProcessing(command, eventData))
            {
#if NET5_0 || NETSTANDARD2_1
                return new(result);
#else
                return Task.FromResult(result);
#endif
            }

#if NET5_0 || NETSTANDARD2_1
            return new ValueTask<InterceptionResult<object>>(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#else
            return Task.FromResult(_processor.ProcessExecutingCommands(command, eventData.Context, result));
#endif
        }
    }
}