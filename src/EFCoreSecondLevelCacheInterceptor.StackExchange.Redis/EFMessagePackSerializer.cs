using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     High-Level API of MessagePack for C#.
/// </summary>
public class EFMessagePackSerializer : IEFDataSerializer
{
    private static readonly IFormatterResolver CustomResolvers = CompositeResolver.Create(
        [EFMessagePackDBNullFormatter.Instance],
        [
            NativeDateTimeResolver.Instance, ContractlessStandardResolver.Instance,
            StandardResolverAllowPrivate.Instance, TypelessContractlessStandardResolver.Instance,
            DynamicGenericResolver.Instance
        ]);

    private readonly bool _enableCompression;
    private MessagePackSerializerOptions? _messagePackSerializerOptions;

    /// <summary>
    ///     High-Level API of MessagePack for C#.
    /// </summary>
    public EFMessagePackSerializer(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
    {
        var options = cacheSettings.Value.AdditionalData as EFRedisCacheConfigurationOptions ??
                      throw new InvalidOperationException(
                          message: "Please call the UseStackExchangeRedisCacheProvider() method.");

        _enableCompression = options.EnableCompression;
    }

    private MessagePackSerializerOptions MessagePackSerializerOptions
        => _messagePackSerializerOptions ??= GetSerializerOptions();

    /// <summary>
    ///     Serializes a given value with the specified buffer writer.
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T? obj) => MessagePackSerializer.Serialize(obj, MessagePackSerializerOptions);

    /// <summary>
    ///     Deserializes a value of a given type from a sequence of bytes.
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? Deserialize<T>(byte[]? data)
        => data is null ? default : MessagePackSerializer.Deserialize<T>(data, MessagePackSerializerOptions);

    private MessagePackSerializerOptions GetSerializerOptions()
        => _enableCompression
            ? MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray)
                .WithResolver(CustomResolvers)
            : MessagePackSerializerOptions.Standard.WithResolver(CustomResolvers);
}