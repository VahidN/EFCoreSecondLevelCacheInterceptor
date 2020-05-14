using System;
using System.Linq;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// EFCachePolicy Parser Utils
    /// </summary>
    public interface IEFCachePolicyParser
    {
        /// <summary>
        /// Converts the `commandText` to an instance of `EFCachePolicy`
        /// </summary>
        EFCachePolicy GetEFCachePolicy(string commandText);

        /// <summary>
        /// Does `commandText` contain nameof(EFCachePolicy)?
        /// </summary>
        bool HasEFCachePolicy(string commandText);

        /// <summary>
        /// Removes the EFCachePolicy line from the commandText
        /// </summary>
        string RemoveEFCachePolicyTag(string commandText);
    }

    /// <summary>
    /// EFCachePolicy Parser Utils
    /// </summary>
    public class EFCachePolicyParser : IEFCachePolicyParser
    {
        internal const string EFCachePolicyTagPrefix = "-- EFCachePolicy";

        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
        private readonly IEFCacheDependenciesProcessor _cacheDependenciesProcessor;
        private readonly IEFDebugLogger _logger;

        /// <summary>
        /// EFCachePolicy Parser Utils
        /// </summary>
        public EFCachePolicyParser(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            IEFCacheDependenciesProcessor cacheDependenciesProcessor,
            IEFDebugLogger logger)
        {
            _cacheSettings = cacheSettings?.Value;
            _cacheDependenciesProcessor = cacheDependenciesProcessor;
            _logger = logger;
        }

        /// <summary>
        /// Does `commandText` contain nameof(EFCachePolicy)?
        /// </summary>
        public bool HasEFCachePolicy(string commandText)
        {
            return !string.IsNullOrWhiteSpace(commandText) && commandText.Contains(nameof(EFCachePolicy));
        }

        /// <summary>
        /// Removes the EFCachePolicy line from the commandText
        /// </summary>
        public string RemoveEFCachePolicyTag(string commandText)
        {
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

            return commandText.Remove(startIndex, (endIndex - startIndex) + 1);
        }

        /// <summary>
        /// Converts the `commandText` to an instance of `EFCachePolicy`
        /// </summary>
        public EFCachePolicy GetEFCachePolicy(string commandText)
        {
            var efCachePolicy = getParsedPolicy(commandText) ?? getGlobalPolicy(commandText);
            if (efCachePolicy != null)
            {
                _logger.LogDebug($"Using EFCachePolicy: {efCachePolicy}.");
            }
            return efCachePolicy;
        }

        private EFCachePolicy getGlobalPolicy(string commandText)
        {
            var cacheAllQueriesOptions = _cacheSettings.CacheAllQueriesOptions;
            return cacheAllQueriesOptions.IsActive
                && !_cacheDependenciesProcessor.IsCrudCommand(commandText)
                && !commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker)
                ? new EFCachePolicy().ExpirationMode(cacheAllQueriesOptions.ExpirationMode).Timeout(cacheAllQueriesOptions.Timeout)
                : null;
        }

        private EFCachePolicy getParsedPolicy(string commandText)
        {
            if (!HasEFCachePolicy(commandText))
            {
                return null;
            }

            var commandTextLines = commandText.Split('\n');
            var efCachePolicyCommentLine = commandTextLines.First(textLine => textLine.StartsWith(EFCachePolicyTagPrefix)).Trim();

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

            if (!TimeSpan.TryParse(options[1], out var timeout))
            {
                return null;
            }

            var saltKey = options.Length >= 3 ? options[2] : string.Empty;

            var cacheDependencies = options.Length >= 4 ? options[3].Split(new[] { EFCachePolicy.CacheDependenciesSeparator }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();

            var isDefaultCacheableMethod = options.Length >= 5 && bool.Parse(options[4]);
            var cacheAllQueriesOptions = _cacheSettings.CacheAllQueriesOptions;
            if (isDefaultCacheableMethod && cacheAllQueriesOptions.IsActive)
            {
                expirationMode = cacheAllQueriesOptions.ExpirationMode;
                timeout = cacheAllQueriesOptions.Timeout;
            }

            return new EFCachePolicy().ExpirationMode(expirationMode).SaltKey(saltKey).Timeout(timeout).CacheDependencies(cacheDependencies);
        }
    }
}