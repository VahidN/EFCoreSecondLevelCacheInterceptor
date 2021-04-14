using System;
using System.Data.Common;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// A custom cache key provider for EF queries.
    /// </summary>
    public class EFCacheKeyProvider : IEFCacheKeyProvider
    {
        private readonly IEFCacheDependenciesProcessor _cacheDependenciesProcessor;
        private readonly IEFDebugLogger _logger;
        private readonly IEFCachePolicyParser _cachePolicyParser;
        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

        /// <summary>
        /// A custom cache key provider for EF queries.
        /// </summary>
        public EFCacheKeyProvider(
            IEFCacheDependenciesProcessor cacheDependenciesProcessor,
            IEFCachePolicyParser cachePolicyParser,
            IEFDebugLogger logger,
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
        {
            _cacheDependenciesProcessor = cacheDependenciesProcessor;
            _logger = logger;
            _cachePolicyParser = cachePolicyParser;

            if (cacheSettings == null)
            {
                throw new ArgumentNullException(nameof(cacheSettings));
            }

            _cacheSettings = cacheSettings.Value;
        }

        /// <summary>
        /// Gets an EF query and returns its hashed key to store in the cache.
        /// </summary>
        /// <param name="command">The EF query.</param>
        /// <param name="context">DbContext is a combination of the Unit Of Work and Repository patterns.</param>
        /// <param name="cachePolicy">determines the Expiration time of the cache.</param>
        /// <returns>Information of the computed key of the input LINQ query.</returns>
        public EFCacheKey GetEFCacheKey(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (cachePolicy == null)
            {
                throw new ArgumentNullException(nameof(cachePolicy));
            }

            var cacheKey = getCacheKey(command, cachePolicy.CacheSaltKey);
            var cacheKeyHash =
                !string.IsNullOrEmpty(_cacheSettings.CacheKeyPrefix) ?
                        $"{_cacheSettings.CacheKeyPrefix}{XxHashUnsafe.ComputeHash(cacheKey):X}" :
                        $"{XxHashUnsafe.ComputeHash(cacheKey):X}";
            var cacheDependencies = _cacheDependenciesProcessor.GetCacheDependencies(command, context, cachePolicy);
            _logger.LogDebug($"KeyHash: {cacheKeyHash}, CacheDependencies: {string.Join(", ", cacheDependencies)}.");
            return new EFCacheKey(cacheDependencies)
            {
                KeyHash = cacheKeyHash
            };
        }

        private string getCacheKey(DbCommand command, string saltKey)
        {
            var cacheKey = new StringBuilder();
            cacheKey.AppendLine(_cachePolicyParser.RemoveEFCachePolicyTag(command.CommandText));

            foreach (DbParameter? parameter in command.Parameters)
            {
                if (parameter == null)
                {
                    continue;
                }

                cacheKey.Append(parameter.ParameterName)
                        .Append('=').Append(getParameterValue(parameter)).Append(',')
                        .Append("Size").Append('=').Append(parameter.Size).Append(',')
                        .Append("Precision").Append('=').Append(parameter.Precision).Append(',')
                        .Append("Scale").Append('=').Append(parameter.Scale).Append(',')
                        .Append("Direction").Append('=').Append(parameter.Direction).Append(',');
            }

            cacheKey.AppendLine("SaltKey").Append('=').Append(saltKey);
            return cacheKey.ToString().Trim();
        }

        private static string? getParameterValue(DbParameter parameter)
        {
            if (parameter.Value is DBNull || parameter.Value is null)
            {
                return "null";
            }

            if (parameter.Value is byte[] buffer)
            {
                return bytesToHex(buffer);
            }

            return parameter.Value?.ToString();
        }

        private static string bytesToHex(byte[] buffer)
        {
            var sb = new StringBuilder(buffer.Length * 2);
            foreach (var @byte in buffer)
            {
                sb.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}