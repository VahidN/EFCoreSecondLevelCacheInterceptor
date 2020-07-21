using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Cache Dependencies Calculator
    /// </summary>
    public interface IEFCacheDependenciesProcessor
    {
        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        SortedSet<string> GetCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy);

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        SortedSet<string> GetCacheDependencies(EFCachePolicy cachePolicy, SortedSet<string> tableNames, string commandText);

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        bool InvalidateCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy);
    }

    /// <summary>
    /// Cache Dependencies Calculator
    /// </summary>
    public class EFCacheDependenciesProcessor : IEFCacheDependenciesProcessor
    {
        private readonly IEFDebugLogger _logger;
        private readonly IEFCacheServiceProvider _cacheServiceProvider;
        private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;

        /// <summary>
        /// Cache Dependencies Calculator
        /// </summary>
        public EFCacheDependenciesProcessor(
            IEFDebugLogger logger,
            IEFCacheServiceProvider cacheServiceProvider,
            IEFSqlCommandsProcessor sqlCommandsProcessor)
        {
            _logger = logger;
            _cacheServiceProvider = cacheServiceProvider;
            _sqlCommandsProcessor = sqlCommandsProcessor;
        }

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        public SortedSet<string> GetCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
        {
            var tableNames = new SortedSet<string>(
                    _sqlCommandsProcessor.GetAllTableNames(context).Select(x => x.TableName));
            return GetCacheDependencies(cachePolicy, tableNames, command.CommandText);
        }

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        public SortedSet<string> GetCacheDependencies(EFCachePolicy cachePolicy, SortedSet<string> tableNames, string commandText)
        {
            var textsInsideSquareBrackets = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);
            var cacheDependencies = new SortedSet<string>(tableNames.Intersect(textsInsideSquareBrackets));
            if (cacheDependencies.Any())
            {
                logProcess(tableNames, textsInsideSquareBrackets, cacheDependencies);
                return cacheDependencies;
            }

            cacheDependencies = cachePolicy.CacheItemsDependencies as SortedSet<string>;
            if (cacheDependencies?.Any() != true)
            {
                _logger.LogDebug($"It's not possible to calculate the related table names of the current query[{commandText}]. Please use EFCachePolicy.Configure(options => options.CacheDependencies(\"real_table_name_1\", \"real_table_name_2\")) to specify them explicitly.");
                cacheDependencies = new SortedSet<string> { EFCachePolicy.EFUnknownsCacheDependency };
            }
            logProcess(tableNames, textsInsideSquareBrackets, cacheDependencies);
            return cacheDependencies;
        }

        private void logProcess(SortedSet<string> tableNames, SortedSet<string> textsInsideSquareBrackets, SortedSet<string> cacheDependencies)
        {
            _logger.LogDebug($"ContextTableNames: {string.Join(", ", tableNames)}, PossibleQueryTableNames: {string.Join(", ", textsInsideSquareBrackets)} -> CacheDependencies: {string.Join(", ", cacheDependencies)}.");
        }

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        public bool InvalidateCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
        {
            var commandText = command.CommandText;
            if (!_sqlCommandsProcessor.IsCrudCommand(commandText))
            {
                return false;
            }

            var cacheDependencies = GetCacheDependencies(command, context, cachePolicy);
            cacheDependencies.Add(EFCachePolicy.EFUnknownsCacheDependency);
            _cacheServiceProvider.InvalidateCacheDependencies(new EFCacheKey { CacheDependencies = cacheDependencies });

            _logger.LogDebug(CacheableEventId.QueryResultInvalidated, $"Invalidated [{string.Join(", ", cacheDependencies)}] dependencies.");
            return true;
        }
    }
}