using System;
using System.Collections.Generic;
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
        private readonly string _keyPrefix;

        /// <summary>
        /// A custom cache key provider for EF queries.
        /// </summary>
        public EFCacheKeyProvider(
            IEFCacheDependenciesProcessor cacheDependenciesProcessor,
            IEFCachePolicyParser cachePolicyParser,
            IEFDebugLogger logger,IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
        {
            _cacheDependenciesProcessor = cacheDependenciesProcessor;
            _logger = logger;
            _cachePolicyParser = cachePolicyParser;
#pragma warning disable CA1062 // Validate arguments of public methods
            _keyPrefix = cacheSettings.Value.CacheKeyPrefix;
#pragma warning restore CA1062 // Validate arguments of public methods
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

            //put the prefix in both places so the namespace will apply regardless of which key is used for access into the cache
            
            
#pragma warning disable S125 // Sections of code should not be commented out
            string cacheKey=string.Empty;
            var cacheKeyHash = string.Empty;
            if (!string.IsNullOrEmpty(_keyPrefix))
            {
                cacheKey += _keyPrefix;
                cacheKeyHash += _keyPrefix;
            }
            cacheKey += getCacheKey(command, cachePolicy.CacheSaltKey);
            cacheKeyHash += $"{XxHashUnsafe.ComputeHash(cacheKey):X}";
            var cacheDependencies = _cacheDependenciesProcessor.GetCacheDependencies(command, context, cachePolicy);

            _logger.LogDebug($"KeyHash: {cacheKeyHash}, CacheDependencies: {string.Join(", ", cacheDependencies)}.");
            return new EFCacheKey(cacheDependencies)
            {
                Key = cacheKey,
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