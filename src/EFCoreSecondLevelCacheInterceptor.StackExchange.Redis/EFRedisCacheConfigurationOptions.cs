using StackExchange.Redis;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     The options relevant to a set of redis connections.
/// </summary>
public class EFRedisCacheConfigurationOptions
{
    /// <summary>
    ///     The options relevant to a set of redis connections.
    /// </summary>
    public ConfigurationOptions? ConfigurationOptions { set; get; }

    /// <summary>
    ///     Redis ConnectionString
    /// </summary>
    public string? RedisConnectionString { set; get; }

    /// <summary>
    ///     Enables MessagePackCompression.Lz4BlockArray
    /// </summary>
    public bool EnableCompression { set; get; }
}