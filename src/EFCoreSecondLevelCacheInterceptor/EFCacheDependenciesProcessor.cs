using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Cache Dependencies Calculator
    /// </summary>
    public class EFCacheDependenciesProcessor : IEFCacheDependenciesProcessor
    {
        private readonly string _cacheKeyPrefix;
        private readonly IEFDebugLogger _logger;
        private readonly IEFCacheServiceProvider _cacheServiceProvider;
        private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;
        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

        /// <summary>
        /// Cache Dependencies Calculator
        /// </summary>
        public EFCacheDependenciesProcessor(
            IEFDebugLogger logger,
            IEFCacheServiceProvider cacheServiceProvider,
            IEFSqlCommandsProcessor sqlCommandsProcessor,
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
        {
            _logger = logger;
            _cacheServiceProvider = cacheServiceProvider;
            _sqlCommandsProcessor = sqlCommandsProcessor;

            if (cacheSettings == null)
            {
                throw new ArgumentNullException(nameof(cacheSettings));
            }

            _cacheSettings = cacheSettings.Value;
            _cacheKeyPrefix = cacheSettings.Value.CacheKeyPrefix;
        }

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        public SortedSet<string> GetCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var tableNames = new SortedSet<string>(
                    _sqlCommandsProcessor.GetAllTableNames(context).Select(x => x.TableName),
                    StringComparer.OrdinalIgnoreCase);
            return GetCacheDependencies(cachePolicy, tableNames, command.CommandText);
        }

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        public SortedSet<string> GetCacheDependencies(EFCachePolicy cachePolicy, SortedSet<string> tableNames, string commandText)
        {
            if (cachePolicy == null)
            {
                throw new ArgumentNullException(nameof(cachePolicy));
            }

            var textsInsideSquareBrackets = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);
            var cacheDependencies = new SortedSet<string>(
                tableNames.Intersect(textsInsideSquareBrackets, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
            if (cacheDependencies.Any())
            {
                logProcess(tableNames, textsInsideSquareBrackets, cacheDependencies);
                return PrefixCacheDependencies(cacheDependencies);
            }

            cacheDependencies = cachePolicy.CacheItemsDependencies as SortedSet<string>;
            if (cacheDependencies?.Any() != true)
            {
                _logger.LogDebug($"It's not possible to calculate the related table names of the current query[{commandText}]. Please use EFCachePolicy.Configure(options => options.CacheDependencies(\"real_table_name_1\", \"real_table_name_2\")) to specify them explicitly.");
                cacheDependencies = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    $"{_cacheKeyPrefix}{EFCachePolicy.EFUnknownsCacheDependency}"
                };
            }
            logProcess(tableNames, textsInsideSquareBrackets, cacheDependencies);
            return PrefixCacheDependencies(cacheDependencies);
        }

        private void logProcess(SortedSet<string> tableNames, SortedSet<string> textsInsideSquareBrackets, SortedSet<string> cacheDependencies)
        {
            _logger.LogDebug($"ContextTableNames: {string.Join(", ", tableNames)}, PossibleQueryTableNames: {string.Join(", ", textsInsideSquareBrackets)} -> CacheDependencies: {string.Join(", ", cacheDependencies)}.");
        }

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        public bool InvalidateCacheDependencies(string commandText, EFCacheKey cacheKey)
        {
            if (cacheKey is null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            if (!_sqlCommandsProcessor.IsCrudCommand(commandText))
            {
                _logger.LogDebug($"Skipped invalidating a none-CRUD command[{commandText}].");
                return false;
            }

            if (shouldSkipCacheInvalidationCommands(commandText))
            {
                _logger.LogDebug($"Skipped invalidating the related cache entries of this query[{commandText}] based on the provided predicate.");
                return false;
            }

            cacheKey.CacheDependencies.Add($"{_cacheKeyPrefix}{EFCachePolicy.EFUnknownsCacheDependency}");
            _cacheServiceProvider.InvalidateCacheDependencies(cacheKey);
            _logger.LogDebug(CacheableEventId.QueryResultInvalidated, $"Invalidated [{string.Join(", ", cacheKey.CacheDependencies)}] dependencies.");
            return true;
        }

        private bool shouldSkipCacheInvalidationCommands(string commandText)
        {
            return _cacheSettings.SkipCacheInvalidationCommands != null && _cacheSettings.SkipCacheInvalidationCommands(commandText);
        }

        private SortedSet<string> PrefixCacheDependencies(SortedSet<string> cacheDependencies)
            => new(cacheDependencies.Select(x => $"{_cacheKeyPrefix}{x}"), StringComparer.OrdinalIgnoreCase);
    }
}