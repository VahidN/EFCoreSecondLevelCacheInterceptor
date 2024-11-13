using System;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
using System.Text.Json;
using Microsoft.Extensions.Options;

#else
using System.Collections;
using System.Globalization;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     A custom cache key provider for EF queries.
/// </summary>
public class EFCacheKeyProvider : IEFCacheKeyProvider
{
#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
    private readonly EFCoreSecondLevelCacheSettings _settings;
#endif

    private readonly IEFCacheDependenciesProcessor _cacheDependenciesProcessor;
    private readonly IEFCacheKeyPrefixProvider _cacheKeyPrefixProvider;
    private readonly IEFCachePolicyParser _cachePolicyParser;

    private readonly IEFHashProvider _hashProvider;
    private readonly ILogger<EFCacheKeyProvider> _keyProviderLogger;
    private readonly IEFDebugLogger _logger;

    /// <summary>
    ///     A custom cache key provider for EF queries.
    /// </summary>
    public EFCacheKeyProvider(IEFCacheDependenciesProcessor cacheDependenciesProcessor,
        IEFCachePolicyParser cachePolicyParser,
        IEFDebugLogger logger,
        ILogger<EFCacheKeyProvider> keyProviderLogger,
        IEFHashProvider hashProvider,
        IEFCacheKeyPrefixProvider cacheKeyPrefixProvider
#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
        ,
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings
#endif
    )
    {
        _cacheDependenciesProcessor = cacheDependenciesProcessor;
        _logger = logger;
        _keyProviderLogger = keyProviderLogger;
        _cachePolicyParser = cachePolicyParser;
        _hashProvider = hashProvider ?? throw new ArgumentNullException(nameof(hashProvider));
        _cacheKeyPrefixProvider = cacheKeyPrefixProvider;
#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
        _settings = cacheSettings?.Value ?? throw new ArgumentNullException(nameof(cacheSettings));
#endif
    }

    /// <summary>
    ///     Gets an EF query and returns its hashed key to store in the cache.
    /// </summary>
    /// <param name="command">The EF query.</param>
    /// <param name="context">DbContext is a combination of the Unit Of Work and Repository patterns.</param>
    /// <param name="cachePolicy">determines the Expiration time of the cache.</param>
    /// <returns>Information of the computed key of the input LINQ query.</returns>
    public EFCacheKey GetEFCacheKey(DbCommand command, DbContext context, EFCachePolicy cachePolicy)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (command is null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (cachePolicy is null)
        {
            throw new ArgumentNullException(nameof(cachePolicy));
        }

        var cacheKey = GetCacheKey(command, cachePolicy.CacheSaltKey);
        var cacheKeyPrefix = _cacheKeyPrefixProvider.GetCacheKeyPrefix();

        var cacheKeyHash = !string.IsNullOrEmpty(cacheKeyPrefix)
            ? $"{cacheKeyPrefix}{_hashProvider.ComputeHash(cacheKey):X}"
            : $"{_hashProvider.ComputeHash(cacheKey):X}";

        var cacheDbContextType = context.GetType();
        var cacheDependencies = _cacheDependenciesProcessor.GetCacheDependencies(command, context, cachePolicy);

        if (_logger.IsLoggerEnabled)
        {
            _keyProviderLogger.LogDebug(
                message: "KeyHash: {CacheKeyHash}, DbContext: {Name}, CacheDependencies: {Dependencies}.", cacheKeyHash,
                cacheDbContextType?.Name, string.Join(separator: ", ", cacheDependencies));
        }

        return new EFCacheKey(cacheDependencies)
        {
            KeyHash = cacheKeyHash,
            DbContext = cacheDbContextType
        };
    }

    private string GetCacheKey(DbCommand command, string saltKey)
    {
        var cacheKey = new StringBuilder();
        cacheKey.AppendLine(_cachePolicyParser.RemoveEFCachePolicyTag(command.CommandText));

        cacheKey.AppendLine(value: "ConnectionString").Append(value: '=').Append(command.Connection?.ConnectionString);

        foreach (DbParameter? parameter in command.Parameters)
        {
            if (parameter == null)
            {
                continue;
            }

            cacheKey.Append(parameter.ParameterName)
                .Append(value: '=')
                .Append(GetParameterValue(parameter))
                .Append(value: ',')
                .Append(value: "Size")
                .Append(value: '=')
                .Append(parameter.Size)
                .Append(value: ',')
                .Append(value: "Precision")
                .Append(value: '=')
                .Append(parameter.Precision)
                .Append(value: ',')
                .Append(value: "Scale")
                .Append(value: '=')
                .Append(parameter.Scale)
                .Append(value: ',')
                .Append(value: "Direction")
                .Append(value: '=')
                .Append(parameter.Direction)
                .Append(value: ',');
        }

        cacheKey.AppendLine(value: "SaltKey").Append(value: '=').Append(saltKey);

        return cacheKey.ToString().Trim();
    }

#if NET9_0 || NET5_0 || NET6_0 || NET7_0 || NET8_0
    private string? GetParameterValue(DbParameter parameter)
        => _settings.JsonSerializerOptions is null
            ? JsonSerializer.Serialize(parameter.Value)
            : JsonSerializer.Serialize(parameter.Value, _settings.JsonSerializerOptions);
#else
    private static string? GetParameterValue(DbParameter parameter)
    {
        return parameter.Value switch
               {
                   DBNull => "null",
                   null => "null",
                   byte[] buffer => BytesToHex(buffer),
                   Array array => EnumerableToString(array),
                   IEnumerable enumerable => EnumerableToString(enumerable),
                   _ => Convert.ToString(parameter.Value, CultureInfo.InvariantCulture),
               };
    }

    private static string EnumerableToString(IEnumerable array)
    {
        var sb = new StringBuilder();
        foreach (var item in array)
        {
            sb.Append(item).Append(' ');
        }

        return sb.ToString();
    }

    private static string BytesToHex(byte[] buffer)
    {
        var sb = new StringBuilder(buffer.Length * 2);
        foreach (var @byte in buffer)
        {
            sb.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }
#endif
}