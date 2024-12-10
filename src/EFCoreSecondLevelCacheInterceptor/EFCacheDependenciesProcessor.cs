using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Cache Dependencies Calculator
/// </summary>
public class EFCacheDependenciesProcessor : IEFCacheDependenciesProcessor
{
    private readonly IEFCacheKeyPrefixProvider _cacheKeyPrefixProvider;
    private readonly IEFCacheServiceProvider _cacheServiceProvider;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly ILogger<EFCacheDependenciesProcessor> _dependenciesProcessorLogger;
    private readonly IEFDebugLogger _logger;
    private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;

    /// <summary>
    ///     Cache Dependencies Calculator
    /// </summary>
    public EFCacheDependenciesProcessor(IEFDebugLogger logger,
        ILogger<EFCacheDependenciesProcessor> dependenciesProcessorLogger,
        IEFCacheServiceProvider cacheServiceProvider,
        IEFSqlCommandsProcessor sqlCommandsProcessor,
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IEFCacheKeyPrefixProvider cacheKeyPrefixProvider)
    {
        _logger = logger;
        _dependenciesProcessorLogger = dependenciesProcessorLogger;
        _cacheServiceProvider = cacheServiceProvider;
        _sqlCommandsProcessor = sqlCommandsProcessor;
        _cacheKeyPrefixProvider = cacheKeyPrefixProvider;

        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheSettings = cacheSettings.Value;
    }

    /// <summary>
    ///     Finds the related table names of the current query.
    /// </summary>
    public SortedSet<string> GetCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var tableNames = new SortedSet<string>(_sqlCommandsProcessor.GetAllTableNames(context).Select(x => x.TableName),
            StringComparer.OrdinalIgnoreCase);

        return GetCacheDependencies(cachePolicy, tableNames, command.CommandText);
    }

    /// <summary>
    ///     Finds the related table names of the current query.
    /// </summary>
    public SortedSet<string> GetCacheDependencies(EFCachePolicy cachePolicy,
        SortedSet<string> tableNames,
        string commandText)
    {
        if (cachePolicy == null)
        {
            throw new ArgumentNullException(nameof(cachePolicy));
        }

        var textsInsideSquareBrackets = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        var cacheDependencies = new SortedSet<string>(
            tableNames.Intersect(textsInsideSquareBrackets, StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);

        if (cacheDependencies.Count != 0)
        {
            LogProcess(tableNames, textsInsideSquareBrackets, cacheDependencies, commandText);

            return PrefixCacheDependencies(cacheDependencies);
        }

        cacheDependencies = cachePolicy.CacheItemsDependencies as SortedSet<string>;

        if (cacheDependencies is { Count: 0 })
        {
            if (_logger.IsLoggerEnabled)
            {
                var message =
                    $"It's not possible to calculate the related table names of the current query[{commandText}]. Please use EFCachePolicy.Configure(options => options.CacheDependencies(\"real_table_name_1\", \"real_table_name_2\")) to specify them explicitly.";

                _dependenciesProcessorLogger.LogDebug(message);
                _logger.NotifyCacheableEvent(CacheableLogEventId.CachingError, message, commandText, efCacheKey: null);
            }

            cacheDependencies = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                EFCachePolicy.UnknownsCacheDependency
            };
        }

        LogProcess(tableNames, textsInsideSquareBrackets, cacheDependencies, commandText);

        return PrefixCacheDependencies(cacheDependencies);
    }

    /// <summary>
    ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    public bool InvalidateCacheDependencies(string commandText, EFCacheKey cacheKey)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        if (!_sqlCommandsProcessor.IsCrudCommand(commandText))
        {
            if (_logger.IsLoggerEnabled)
            {
                var message = $"Skipped invalidating a none-CRUD command[{commandText}].";
                _dependenciesProcessorLogger.LogDebug(message);
                _logger.NotifyCacheableEvent(CacheableLogEventId.InvalidationSkipped, message, commandText, cacheKey);
            }

            return false;
        }

        if (ShouldSkipCacheInvalidationCommands(commandText))
        {
            if (_logger.IsLoggerEnabled)
            {
                var message =
                    $"Skipped invalidating the related cache entries of this query[{commandText}] based on the provided predicate.";

                _dependenciesProcessorLogger.LogDebug(message);
                _logger.NotifyCacheableEvent(CacheableLogEventId.InvalidationSkipped, message, commandText, cacheKey);
            }

            return false;
        }

        var cacheKeyPrefix = _cacheKeyPrefixProvider.GetCacheKeyPrefix();
        cacheKey.CacheDependencies.Add($"{cacheKeyPrefix}{EFCachePolicy.UnknownsCacheDependency}");
        _cacheServiceProvider.InvalidateCacheDependencies(cacheKey);

        if (_logger.IsLoggerEnabled)
        {
            var message = $"Invalidated [{string.Join(separator: ", ", cacheKey.CacheDependencies)}] dependencies.";
            _dependenciesProcessorLogger.LogDebug(CacheableEventId.QueryResultInvalidated, message);
            _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultInvalidated, message, commandText, cacheKey);
        }

        _logger.NotifyCacheInvalidation(clearAllCachedEntries: false, cacheKey.CacheDependencies);

        return true;
    }

    private void LogProcess(SortedSet<string> tableNames,
        SortedSet<string> textsInsideSquareBrackets,
        SortedSet<string>? cacheDependencies,
        string commandText)
    {
        if (_logger.IsLoggerEnabled)
        {
            var names = string.Join(separator: ", ", tableNames);

            var texts = string.Join(separator: ", ",
                cacheDependencies ?? new SortedSet<string>(StringComparer.Ordinal));

            var dependencies = string.Join(separator: ", ", textsInsideSquareBrackets);

            var message =
                $"ContextTableNames: {names}, PossibleQueryTableNames: {texts} -> CacheDependencies: {dependencies}.";

            _dependenciesProcessorLogger.LogDebug(message);

            _logger.NotifyCacheableEvent(CacheableLogEventId.CacheDependenciesCalculated, message, commandText,
                efCacheKey: null);
        }
    }

    private bool ShouldSkipCacheInvalidationCommands(string commandText)
        => _cacheSettings.SkipCacheInvalidationCommands != null &&
           _cacheSettings.SkipCacheInvalidationCommands(commandText);

    private SortedSet<string> PrefixCacheDependencies(SortedSet<string>? cacheDependencies)
    {
        if (cacheDependencies is null)
        {
            return new SortedSet<string>(StringComparer.Ordinal);
        }

        var cacheKeyPrefix = _cacheKeyPrefixProvider.GetCacheKeyPrefix();

        return new SortedSet<string>(cacheDependencies.Select(x => $"{cacheKeyPrefix}{x}"),
            StringComparer.OrdinalIgnoreCase);
    }
}