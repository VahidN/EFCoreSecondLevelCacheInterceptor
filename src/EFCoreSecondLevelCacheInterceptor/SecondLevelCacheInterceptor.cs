using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
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
        private IDbCommandInterceptorProcessor _processor;

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQuery
        /// </summary>
        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            if (eventData.Context == null)
            {
                return base.NonQueryExecuted(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteNonQueryAsync.
        /// </summary>
        public override Task<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result));
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQuery.
        /// </summary>
        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context == null)
            {
                return base.NonQueryExecuting(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteNonQueryAsync.
        /// </summary>
        public override Task<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result));
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReader.
        /// </summary>
        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            if (eventData.Context == null)
            {
                return base.ReaderExecuted(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteReaderAsync.
        /// </summary>
        public override Task<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result));
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReader.
        /// </summary>
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            if (eventData.Context == null)
            {
                return base.ReaderExecuting(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteReaderAsync.
        /// </summary>
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result));
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalar.
        /// </summary>
        public override object ScalarExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result)
        {
            if (eventData.Context == null)
            {
                return base.ScalarExecuted(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called immediately after EF calls System.Data.Common.DbCommand.ExecuteScalarAsync.
        /// </summary>
        public override Task<object> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutedCommands(command, eventData.Context, result));
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalar.
        /// </summary>
        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result)
        {
            if (eventData.Context == null)
            {
                return base.ScalarExecuting(command, eventData, result);
            }

            return getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result);
        }

        /// <summary>
        /// Called just before EF intends to call System.Data.Common.DbCommand.ExecuteScalarAsync.
        /// </summary>
        public override Task<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null)
            {
                return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
            }

            return Task.FromResult(getProcessor(eventData.Context).ProcessExecutingCommands(command, eventData.Context, result));
        }

        private IDbCommandInterceptorProcessor getProcessor(DbContext context)
        {
            if (_processor != null)
            {
                return _processor;
            }
            _processor = context.GetService<IDbCommandInterceptorProcessor>();
            if (_processor == null)
            {
                throw new InvalidOperationException("Please add `AddEFSecondLevelCache()` method to your `IServiceCollection`.");
            }
            return _processor;
        }
    }
}