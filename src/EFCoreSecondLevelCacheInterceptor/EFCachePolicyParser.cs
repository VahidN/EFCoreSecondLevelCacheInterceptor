using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// EFCachePolicy Parser Utils
    /// </summary>
    public class EFCachePolicyParser : IEFCachePolicyParser
    {
        /// <summary>
        /// EFCachePolicy Tag Prefix
        /// </summary>
        public static readonly string EFCachePolicyTagPrefix = $"-- {nameof(EFCachePolicy)}";

        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
        private readonly IEFSqlCommandsProcessor _sqlCommandsProcessor;
        private readonly IEFDebugLogger _logger;

        private static readonly HashSet<string> _nonDeterministicFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        /// <summary>
        /// EFCachePolicy Parser Utils
        /// </summary>
        public EFCachePolicyParser(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            IEFSqlCommandsProcessor sqlCommandsProcessor,
            IEFDebugLogger logger)
        {
            if (cacheSettings == null)
            {
                throw new ArgumentNullException(nameof(cacheSettings));
            }

            _cacheSettings = cacheSettings.Value;
            _sqlCommandsProcessor = sqlCommandsProcessor ?? throw new ArgumentNullException(nameof(sqlCommandsProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Does `commandText` contain EFCachePolicyTagPrefix?
        /// </summary>
        public bool HasEFCachePolicy(string commandText)
        {
            return !string.IsNullOrWhiteSpace(commandText)
                    && commandText.Contains(EFCachePolicyTagPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Removes the EFCachePolicy line from the commandText
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

            var endIndex = commandText.IndexOf('\n', startIndex);
            if (endIndex == -1)
            {
                return commandText;
            }

            var additionalNewlineIndex = commandText.IndexOf('\n', endIndex + 1) - endIndex;
            if (additionalNewlineIndex == 1 || additionalNewlineIndex == 2)
            {
                // EF's TagWith(..) method inserts an additional line break between
                // comments which we can remove as well
                endIndex += additionalNewlineIndex;
            }

            return commandText.Remove(startIndex, (endIndex - startIndex) + 1);
        }

        /// <summary>
        /// Converts the `commandText` to an instance of `EFCachePolicy`
        /// </summary>
        public EFCachePolicy? GetEFCachePolicy(string commandText, IList<TableEntityInfo> allEntityTypes)
        {
            if (commandText == null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (containsNonDeterministicFunction(commandText))
            {
                return null;
            }

            if (shouldSkipCachingCommands(commandText))
            {
                return null;
            }

            var efCachePolicy = getParsedPolicy(commandText)
                                    ?? getSpecificGlobalPolicy(commandText, allEntityTypes)
                                    ?? getSkippedGlobalPolicy(commandText, allEntityTypes)
                                    ?? getGlobalPolicy(commandText);
            if (efCachePolicy != null)
            {
                _logger.LogDebug($"Using EFCachePolicy: {efCachePolicy}.");
            }
            return efCachePolicy;
        }

        private bool shouldSkipCachingCommands(string commandText)
        {
            var result = _cacheSettings.SkipCachingCommands != null && _cacheSettings.SkipCachingCommands(commandText);
            if (result)
            {
                _logger.LogDebug($"Skipped caching of this command[{commandText}] based on the provided predicate.");
            }
            return result;
        }

        private EFCachePolicy? getSpecificGlobalPolicy(string commandText, IList<TableEntityInfo> allEntityTypes)
        {
            var options = _cacheSettings.CacheSpecificQueriesOptions;
            if (options?.IsActive != true
                    || _sqlCommandsProcessor.IsCrudCommand(commandText)
                    || commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal))
            {
                return null;
            }

            var shouldBeCached = false;
            if (options.TableNames != null)
            {
                var commandTableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);
                if (options.TableNames.Any(tableName => commandTableNames.Contains(tableName, StringComparer.OrdinalIgnoreCase)))
                {
                    shouldBeCached = true;
                }
            }

            if (options.EntityTypes != null)
            {
                var queryEntityTypes = _sqlCommandsProcessor.GetSqlCommandEntityTypes(commandText, allEntityTypes);
                if (queryEntityTypes.Any(entityType => options.EntityTypes.Contains(entityType)))
                {
                    shouldBeCached = true;
                }
            }

            return shouldBeCached
                ? new EFCachePolicy().ExpirationMode(options.ExpirationMode).Timeout(options.Timeout)
                : null;
        }

        private EFCachePolicy? getSkippedGlobalPolicy(string commandText, IList<TableEntityInfo> allEntityTypes)
        {
            var options = _cacheSettings.SkipCacheSpecificQueriesOptions;
            if (options?.IsActive != true
                    || _sqlCommandsProcessor.IsCrudCommand(commandText)
                    || commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal))
            {
                return null;
            }

            var shouldBeCached = true;
            if (options.TableNames != null)
            {
                var commandTableNames = _sqlCommandsProcessor.GetSqlCommandTableNames(commandText);
                if (options.TableNames.Any(tableName => commandTableNames.Contains(tableName, StringComparer.OrdinalIgnoreCase)))
                {
                    shouldBeCached = false;
                }
            }

            if (options.EntityTypes != null)
            {
                var queryEntityTypes = _sqlCommandsProcessor.GetSqlCommandEntityTypes(commandText, allEntityTypes);
                if (queryEntityTypes.Any(entityType => options.EntityTypes.Contains(entityType)))
                {
                    shouldBeCached = false;
                }
            }

            return shouldBeCached
                ? new EFCachePolicy().ExpirationMode(options.ExpirationMode).Timeout(options.Timeout)
                : null;
        }

        private EFCachePolicy? getGlobalPolicy(string commandText)
        {
            var cacheAllQueriesOptions = _cacheSettings.CacheAllQueriesOptions;
            return cacheAllQueriesOptions.IsActive
                && !_sqlCommandsProcessor.IsCrudCommand(commandText)
                && !commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker, StringComparison.Ordinal)
                ? new EFCachePolicy().ExpirationMode(cacheAllQueriesOptions.ExpirationMode).Timeout(cacheAllQueriesOptions.Timeout)
                : null;
        }

        private EFCachePolicy? getParsedPolicy(string commandText)
        {
            if (!HasEFCachePolicy(commandText))
            {
                return null;
            }

            var commandTextLines = commandText.Split('\n');
            var efCachePolicyCommentLine = commandTextLines.First(textLine => textLine.StartsWith(EFCachePolicyTagPrefix, StringComparison.Ordinal)).Trim();

            var parts = efCachePolicyCommentLine.Split(new[] { EFCachePolicy.PartsSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            var options = parts[1].Split(new[] { EFCachePolicy.ItemsSeparator }, StringSplitOptions.None);
            if (options.Length < 2)
            {
                return null;
            }

            if (!Enum.TryParse<CacheExpirationMode>(options[0], out var expirationMode))
            {
                return null;
            }

            if (!TimeSpan.TryParse(options[1], CultureInfo.InvariantCulture, out var timeout))
            {
                return null;
            }

            var saltKey = options.Length >= 3 ? options[2] : string.Empty;

            var cacheDependencies = options.Length >= 4 ? options[3].Split(new[] { EFCachePolicy.CacheDependenciesSeparator }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();

            var isDefaultCacheableMethod = options.Length >= 5 && bool.Parse(options[4]);
            if (isDefaultCacheableMethod && _cacheSettings.CacheAllQueriesOptions.IsActive)
            {
                expirationMode = _cacheSettings.CacheAllQueriesOptions.ExpirationMode;
                timeout = _cacheSettings.CacheAllQueriesOptions.Timeout;
            }
            else if (isDefaultCacheableMethod && _cacheSettings.CachableQueriesOptions.IsActive)
            {
                expirationMode = _cacheSettings.CachableQueriesOptions.ExpirationMode;
                timeout = _cacheSettings.CachableQueriesOptions.Timeout;
            }

            return new EFCachePolicy().ExpirationMode(expirationMode).SaltKey(saltKey).Timeout(timeout).CacheDependencies(cacheDependencies);
        }

        private bool containsNonDeterministicFunction(string commandText)
        {
            return _nonDeterministicFunctions.Any(item =>
            {
                var hasFn = commandText.Contains(item, StringComparison.OrdinalIgnoreCase);
                if (hasFn)
                {
                    _logger.LogDebug($"Skipped caching because of the non-deterministic function -> `{item}`.");
                }
                return hasFn;
            });
        }
    }
}