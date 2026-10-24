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
    public ConfigurationOptions? ConfigurationOptions { get; set; }

    /// <summary>
    ///     Redis ConnectionString
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    ///     Provides a DI-backed connection factory.
    /// </summary>
    public Func<IServiceProvider, IConnectionMultiplexer>? ConnectionMultiplexerFactory { get; set; }

    /// <summary>
    ///     Enables MessagePackCompression.Lz4BlockArray
    /// </summary>
    public bool EnableCompression { get; set; }
}