using System;
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
        private readonly CacheAllQueriesOptions _cacheAllQueriesOptions;
        private readonly IEFCacheDependenciesProcessor _cacheDependenciesProcessor;

        /// <summary>
        /// EFCachePolicy Parser Utils
        /// </summary>
        public EFCachePolicyParser(
            IOptions<CacheAllQueriesOptions> cacheAllQueriesOptions,
            IEFCacheDependenciesProcessor cacheDependenciesProcessor)
        {
            _cacheAllQueriesOptions = cacheAllQueriesOptions?.Value;
            _cacheDependenciesProcessor = cacheDependenciesProcessor;
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
            var startIndex = commandText.IndexOf(nameof(EFCachePolicy), StringComparison.Ordinal);
            if (startIndex == -1)
            {
                return commandText;
            }

            var endIndex = commandText.IndexOf('\n');
            if (endIndex == -1)
            {
                return commandText;
            }

            return commandText.Remove(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Converts the `commandText` to an instance of `EFCachePolicy`
        /// </summary>
        public EFCachePolicy GetEFCachePolicy(string commandText)
        {
            return getParsedPolicy(commandText) ?? getGlobalPolicy(commandText);
        }

        private EFCachePolicy getGlobalPolicy(string commandText)
        {
            return _cacheAllQueriesOptions?.IsActive == true
                && !_cacheDependenciesProcessor.IsCrudCommand(commandText)
                && !commandText.Contains(EFCachedQueryExtensions.IsNotCachableMarker)
                ? new EFCachePolicy().ExpirationMode(_cacheAllQueriesOptions.ExpirationMode).Timeout(_cacheAllQueriesOptions.Timeout)
                : null;
        }

        private EFCachePolicy getParsedPolicy(string commandText)
        {
            if (!HasEFCachePolicy(commandText))
            {
                return null;
            }

            var firstLine = commandText.Split('\n')[0].Trim();

            var parts = firstLine.Split(new[] { EFCachePolicy.PartsSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            var options = parts[1].Split(new[] { EFCachePolicy.ItemsSeparator }, StringSplitOptions.RemoveEmptyEntries);
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

            var cacheDependencies = options.Length == 4 ? options[3].Split(new[] { EFCachePolicy.CacheDependenciesSeparator }, StringSplitOptions.RemoveEmptyEntries) : Array.Empty<string>();

            return new EFCachePolicy().ExpirationMode(expirationMode).SaltKey(saltKey).Timeout(timeout).CacheDependencies(cacheDependencies);
        }
    }
}