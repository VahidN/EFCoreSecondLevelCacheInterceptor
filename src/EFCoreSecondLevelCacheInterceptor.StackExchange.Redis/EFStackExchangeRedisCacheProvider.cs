using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Stack Exchange Redis Cache Provider
/// </summary>
public class EFStackExchangeRedisCacheProvider(
    IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
    IEFDebugLogger logger,
    IEFDataSerializer dataSerializer) : IEFCacheServiceProvider
{
    private ConnectionMultiplexer? _redisConnection;

    private ConnectionMultiplexer RedisConnection => _redisConnection ??= GetRedisConnection();

    /// <inheritdoc />
    public void InsertValue(EFCacheKey cacheKey, EFCachedData? value, EFCachePolicy cachePolicy)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        if (cachePolicy is null)
        {
            throw new ArgumentNullException(nameof(cachePolicy));
        }

        value ??= new EFCachedData
        {
            IsNull = true
        };

        var redisDb = RedisConnection.GetDatabase();
        var keyHash = cacheKey.KeyHash;

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }

            var expiryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() +
                             cachePolicy.CacheTimeout.TotalMilliseconds;

            redisDb.SortedSetAdd(rootCacheKey, keyHash, expiryTime);
        }

        var data = dataSerializer.Serialize(value);

        redisDb.StringSet(keyHash, data, cachePolicy.CacheTimeout);
    }

    /// <inheritdoc />
    public void ClearAllCachedEntries()
        => logger.NotifyCacheInvalidation(clearAllCachedEntries: true,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    /// <inheritdoc />
    public EFCachedData? GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        var redisDb = RedisConnection.GetDatabase();
        var maybeValue = redisDb.StringGet(cacheKey.KeyHash);

        return maybeValue.HasValue ? dataSerializer.Deserialize<EFCachedData>(maybeValue) : null;
    }

    /// <inheritdoc />
    public void InvalidateCacheDependencies(EFCacheKey cacheKey)
    {
        if (cacheKey is null)
        {
            throw new ArgumentNullException(nameof(cacheKey));
        }

        var redisDb = RedisConnection.GetDatabase();

        foreach (var rootCacheKey in cacheKey.CacheDependencies)
        {
            if (string.IsNullOrWhiteSpace(rootCacheKey))
            {
                continue;
            }

            var dependencyKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in redisDb.SortedSetScan(rootCacheKey))
            {
                _ = dependencyKeys.Add(item.Element.ToString());
            }

            if (dependencyKeys.Count > 0)
            {
                redisDb.KeyDelete([.. dependencyKeys]);
            }

            redisDb.KeyDelete(rootCacheKey);
        }
    }

    private ConnectionMultiplexer GetRedisConnection()
    {
        var options = cacheSettings.Value.AdditionalData as EFRedisCacheConfigurationOptions ??
                      throw new InvalidOperationException(
                          message: "Please call the UseStackExchangeRedisCacheProvider() method.");

        if (options.RedisConnectionString is not null)
        {
            return ConnectionMultiplexer.Connect(options.RedisConnectionString);
        }

        if (options.ConfigurationOptions is not null)
        {
            return ConnectionMultiplexer.Connect(options.ConfigurationOptions);
        }

        throw new InvalidOperationException(
            message:
            "Please specify the `RedisConnectionString` or `ConfigurationOptions` by calling the UseStackExchangeRedisCacheProvider() method.");
    }
}