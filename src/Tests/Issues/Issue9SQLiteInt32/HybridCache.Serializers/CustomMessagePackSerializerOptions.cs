using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace Issue9SQLiteInt32.HybridCache.Serializers;

public static class CustomMessagePackSerializerOptions
{
    private static readonly IFormatterResolver CustomResolvers = CompositeResolver.Create(
        [
            MessagePackDBNullFormatter.Instance, NativeDateTimeArrayFormatter.Instance,
            NativeDateTimeFormatter.Instance,
            NativeDecimalFormatter.Instance, NativeGuidFormatter.Instance, TypelessFormatter.Instance
        ],
        [
            NativeDateTimeResolver.Instance, NativeDecimalResolver.Instance, NativeGuidResolver.Instance,
            ContractlessStandardResolver.Instance, StandardResolverAllowPrivate.Instance,
            TypelessContractlessStandardResolver.Instance, DynamicGenericResolver.Instance
        ]);

    public static readonly MessagePackSerializerOptions SerializerOptions = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4BlockArray)
        .WithResolver(CustomResolvers);
}