using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     EFCachePolicy Parser Utils
/// </summary>
public class EFCachePolicyParser : IEFCachePolicyParser
{
    /// <summary>
    ///     EFCachePolicy Tag Prefix
    /// </summary>
    public static readonly string EFCachePolicyTagPrefix = $"-- {nameof(EFCachePolicy)}";

    private static readonly HashSet<string> NonDeterministicFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "NEWID()",
        "GETDATE()",
        "GETUTCDATE()",
        "SYSDATETIME()",
        "SYSUTCDATETIME()",
        "SYSDATETIMEOFFSET()",
        "CURRENT_USER()",
        "CURRENT_TIMESTAMP()",
        "HOST_NAME()",
        "USER_NAME()",
        "NOW()",
        "getguid()",
        "uuid_generate_v4()",
        "current_timestamp",
        "current_date",
        "current_time"
    };

    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly IEFDebugLogger _logger;
    private readonly ILogger<EFCachePolicyParser> _policyParserLogger;
    private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;

    /// <summary>
    ///     EFCachePolicy Parser Utils
    /// </summary>
    public EFCachePolicyParser(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IEFSqlCommandsProcessor sqlCommandsProcessor,
        IEFDebugLogger logger,
        ILogger<EFCachePolicyParser> policyParserLogger)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheSettings = cacheSettings.Value;
        _sqlCommandsProcessor = sqlCommandsProcessor ?? throw new ArgumentNullException(nameof(sqlCommandsProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _policyParserLogger = policyParserLogger;
    }

    /// <summary>
    ///     Does `commandText` contain EFCachePolicyTagPrefix?
    /// </summary>
    public bool HasEFCachePolicy(string commandText)
        => !string.IsNullOrWhiteSpace(commandText) &&
           commandText.Contains(EFCachePolicyTagPrefix, StringComparison.Ordinal);

    /// <summary>
    ///     Removes the EFCachePolicy line from the commandText
    /// </summary>
    public string RemoveEFCachePolicyTag(string commandText)
    {
        if (commandText == null)
        {
            throw new ArgumentNullException(nameof(commandText));
        }

        var startIndex = commandText.IndexOf(EFCachePolicyTagPrefix, StringComparison.Ordinal);

        if (startIndex == -1)
        {
            return commandText;
        }

        var endIndex = commandText.IndexOf(value: '\n', startIndex);

        if (endIndex == -1)
        {
            return commandText;
        }

        var additionalNewlineIndex = commandText.IndexOf(value: '\n', endIndex + 1) - endIndex;

        if (additionalNewlineIndex is 1 or 2)
        {
            // EF's TagWith(..) method inserts an additional line break between
            // comments which we can remove as well
            endIndex += additionalNewlineIndex;
        }

        return commandText.Remove(startIndex, endIndex - startIndex + 1);
    }

    /// <summary>
    ///     Converts the `commandText` to an instance of `EFCachePolicy`
    /// </summary>
    public (EFCachePolicy? CachePolicy, bool ShouldSkipCaching) GetEFCachePolicy(string commandText,
        IList<TableEntityInfo> allEntityTypes)
    {
        if (commandText == null)
        {
            throw new ArgumentNullException(nameof(commandText));
        }

        if (ContainsNonDeterministicFunction(commandText) || ShouldSkipCachingCommands(commandText))
        {
            return (null, true);
        }

        var efCachePolicy = GetParsedPolicy(commandText) ?? GetSpecificGlobalPolicy(commandText, allEntityTypes) ??
            GetSkippedGlobalPolicy(commandText, allEntityTypes) ?? GetGlobalPolicy(commandText);

        efCachePolicy = TryOverrideCachePolicy(efCachePolicy, commandText, allEntityTypes);

        if (efCachePolicy != null && _logger.IsLoggerEnabled)
        {
            var message = $"Using EFCachePolicy: {efCachePolicy}.";
            _policyParserLogger.LogDebug(message: "Using EFCachePolicy: {EfCachePolicy}.", efCachePolicy);

            _logger.NotifyCacheableEvent(CacheableLogEventId.CachePolicyCalculated, message, commandText,
                efCacheKey: null);
        }

        return (efCachePolicy, false);
    }

    private EFCachePolicy? TryOverrideCachePolicy(EFCachePolicy? efCachePolicy,
        string commandText,
        IList<TableEntityInfo> allEntityTypes)
    {
        if (_cacheSettings.OverrideCachePolicy is null)
        {
            return efCachePolicy;
        }

        var newCachePolicy = _cacheSettings.OverrideCachePolicy(new EFCachePolicyContext
        {
            CommandText = commandText,
            CommandTableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText),
            CommandEntityTypes = _sqlCommandsProcessor.GetSqlCommandEntityTypes(commandText, allEntityTypes),
            IsCrudCommand = _sqlCommandsProcessor.IsCrudCommand(commandText),
            CommandCachePolicy = efCachePolicy
        });

        if (newCachePolicy is not null)
        {
            efCachePolicy = newCachePolicy;
        }

        return efCachePolicy;
    }

    private bool ShouldSkipCachingCommands(string commandText)
    {
        var result = _cacheSettings.SkipCachingCommands != null && _cacheSettings.SkipCachingCommands(commandText);

        if (result && _logger.IsLoggerEnabled)
        {
            var message = $"Skipped caching of this command[{commandText}] based on the provided predicate.";
            _policyParserLogger.LogDebug(message);
            _logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText, efCacheKey: null);
        }

        return result;
    }

    private EFCachePolicy? GetSpecificGlobalPolicy(string commandText, IList<TableEntityInfo> allEntityTypes)
    {
        var options = _cacheSettings.CacheSpecificQueriesOptions;

        if (options?.IsActive != true || _sqlCommandsProcessor.IsCrudCommand(commandText) ||
            commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal))
        {
            return null;
        }

        var shouldBeCached = false;

        if (options.TableNames != null)
        {
            shouldBeCached = ShouldCacheQueriesContainingTableNames(commandText, options);
        }

        if (options.EntityTypes != null)
        {
            shouldBeCached = ShouldCacheQueriesContainingTableTypes(commandText, options, allEntityTypes);
        }

        return shouldBeCached
            ? new EFCachePolicy().ExpirationMode(options.ExpirationMode).Timeout(options.Timeout)
            : null;
    }

    private bool ShouldCacheQueriesContainingTableTypes(string commandText,
        CacheSpecificQueriesOptions options,
        IList<TableEntityInfo> allEntityTypes)
    {
        if (options.EntityTypes is null || options.EntityTypes.Count == 0)
        {
            return false;
        }

        var queryEntityTypes = _sqlCommandsProcessor.GetSqlCommandEntityTypes(commandText, allEntityTypes);

        if (queryEntityTypes.Count == 0)
        {
            return false;
        }

        switch (options.TableTypeComparison)
        {
            case TableTypeComparison.Contains:
                if (queryEntityTypes.Any(options.EntityTypes.Contains))
                {
                    return true;
                }

                break;
            case TableTypeComparison.DoesNotContain:
                if (queryEntityTypes.Any(entityType => !options.EntityTypes.Contains(entityType)))
                {
                    return true;
                }

                break;
            case TableTypeComparison.ContainsEvery:
                if (queryEntityTypes.OrderBy(x => x.FullName, StringComparer.Ordinal)
                    .SequenceEqual(options.EntityTypes.OrderBy(x => x.FullName, StringComparer.Ordinal)))
                {
                    return true;
                }

                break;
            case TableTypeComparison.ContainsOnly:
                if (queryEntityTypes.All(options.EntityTypes.Contains))
                {
                    return true;
                }

                break;
            case TableTypeComparison.DoesNotContainEvery:
                if (!queryEntityTypes.OrderBy(x => x.FullName, StringComparer.Ordinal)
                        .SequenceEqual(options.EntityTypes.OrderBy(x => x.FullName, StringComparer.Ordinal)))
                {
                    return true;
                }

                break;
        }

        return false;
    }

    private bool ShouldCacheQueriesContainingTableNames(string commandText, CacheSpecificQueriesOptions options)
    {
        if (options.TableNames is null || !options.TableNames.Any())
        {
            return false;
        }

        var commandTableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

        if (commandTableNames.Count == 0)
        {
            return false;
        }

        switch (options.TableNameComparison)
        {
            case TableNameComparison.Contains:
                if (options.TableNames.Any(commandTableNames.Contains))
                {
                    return true;
                }

                break;
            case TableNameComparison.DoesNotContain:
                if (options.TableNames.Any(tableName => !commandTableNames.Contains(tableName)))
                {
                    return true;
                }

                break;
            case TableNameComparison.EndsWith:
                if (options.TableNames.Any(tableName
                        => commandTableNames.EndsWith(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                break;
            case TableNameComparison.DoesNotEndWith:
                if (options.TableNames.Any(tableName
                        => !commandTableNames.EndsWith(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                break;
            case TableNameComparison.StartsWith:
                if (options.TableNames.Any(tableName
                        => commandTableNames.StartsWith(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                break;
            case TableNameComparison.DoesNotStartWith:
                if (options.TableNames.Any(tableName
                        => !commandTableNames.StartsWith(tableName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                break;
            case TableNameComparison.ContainsEvery:
                if (commandTableNames.ContainsEvery(options.TableNames, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                break;
            case TableNameComparison.ContainsOnly:
                if (commandTableNames.ContainsOnly(options.TableNames, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                break;
            case TableNameComparison.DoesNotContainEvery:
                if (!commandTableNames.ContainsEvery(options.TableNames, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                break;
        }

        return false;
    }

    private EFCachePolicy? GetSkippedGlobalPolicy(string commandText, IList<TableEntityInfo> allEntityTypes)
    {
        var options = _cacheSettings.SkipCacheSpecificQueriesOptions;

        if (options?.IsActive != true || _sqlCommandsProcessor.IsCrudCommand(commandText) ||
            commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal))
        {
            return null;
        }

        var shouldBeCached = true;

        if (options.TableNames != null)
        {
            var commandTableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);

            if (options.TableNames.Any(commandTableNames.Contains))
            {
                shouldBeCached = false;
            }
        }

        if (options.EntityTypes != null)
        {
            var queryEntityTypes = _sqlCommandsProcessor.GetSqlCommandEntityTypes(commandText, allEntityTypes);

            if (queryEntityTypes.Any(options.EntityTypes.Contains))
            {
                shouldBeCached = false;
            }
        }

        return shouldBeCached
            ? new EFCachePolicy().ExpirationMode(options.ExpirationMode).Timeout(options.Timeout)
            : null;
    }

    private EFCachePolicy? GetGlobalPolicy(string commandText)
    {
        var cacheAllQueriesOptions = _cacheSettings.CacheAllQueriesOptions;

        return cacheAllQueriesOptions.IsActive && !_sqlCommandsProcessor.IsCrudCommand(commandText) &&
               !commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal)
            ? new EFCachePolicy().ExpirationMode(cacheAllQueriesOptions.ExpirationMode)
                .Timeout(cacheAllQueriesOptions.Timeout)
            : null;
    }

    private EFCachePolicy? GetParsedPolicy(string commandText)
    {
        if (!HasEFCachePolicy(commandText))
        {
            return null;
        }

        var commandTextLines = commandText.Split(separator: '\n');

        var efCachePolicyCommentLine = commandTextLines
            .First(textLine => textLine.StartsWith(EFCachePolicyTagPrefix, StringComparison.Ordinal))
            .Trim();

        var parts = efCachePolicyCommentLine.Split([EFCachePolicy.PartsSeparator],
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
        {
            return null;
        }

        var options = parts[1].Split([EFCachePolicy.ItemsSeparator], StringSplitOptions.None);

        if (options.Length < 2)
        {
            return null;
        }

        if (!Enum.TryParse<CacheExpirationMode>(options[0], out var expirationMode))
        {
            return null;
        }

        TimeSpan? cacheTimeout = null;

        if (TimeSpan.TryParse(options[1], CultureInfo.InvariantCulture, out var timeout))
        {
            cacheTimeout = timeout;
        }

        var saltKey = options.Length >= 3 ? options[2] : string.Empty;

        var cacheDependencies = options.Length >= 4
            ? options[3].Split([EFCachePolicy.CacheDependenciesSeparator], StringSplitOptions.RemoveEmptyEntries)
            : [];

        var isDefaultCacheableMethod = options.Length >= 5 && bool.Parse(options[4]);

        if (isDefaultCacheableMethod && _cacheSettings.CacheAllQueriesOptions.IsActive)
        {
            expirationMode = _cacheSettings.CacheAllQueriesOptions.ExpirationMode;
            cacheTimeout = _cacheSettings.CacheAllQueriesOptions.Timeout;
        }
        else if (isDefaultCacheableMethod && _cacheSettings.CachableQueriesOptions.IsActive)
        {
            expirationMode = _cacheSettings.CachableQueriesOptions.ExpirationMode;
            cacheTimeout = _cacheSettings.CachableQueriesOptions.Timeout;
        }

        return new EFCachePolicy().ExpirationMode(expirationMode)
            .SaltKey(saltKey)
            .Timeout(cacheTimeout)
            .CacheDependencies(cacheDependencies);
    }

    private bool ContainsNonDeterministicFunction(string commandText)
        => NonDeterministicFunctions.Any(item =>
        {
            var hasFn = commandText.Contains(item, StringComparison.OrdinalIgnoreCase);

            if (hasFn && _logger.IsLoggerEnabled)
            {
                var message = $"Skipped caching because of the non-deterministic function -> `{item}`.";
                _policyParserLogger.LogDebug(message);

                _logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText,
                    efCacheKey: null);
            }

            return hasFn;
        });
}