using MessagePack;
using MessagePack.Resolvers;

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

    /// <summary>
    ///     Serializes a given value with the specified buffer writer.
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T? obj)
        => MessagePackSerializer.Serialize(obj, MessagePackSerializerOptions.Standard.WithResolver(CustomResolvers));

    /// <summary>
    ///     Deserializes a value of a given type from a sequence of bytes.
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? Deserialize<T>(byte[]? data)
        => data is null
            ? default
            : MessagePackSerializer.Deserialize<T>(data,
                MessagePackSerializerOptions.Standard.WithResolver(CustomResolvers));
}