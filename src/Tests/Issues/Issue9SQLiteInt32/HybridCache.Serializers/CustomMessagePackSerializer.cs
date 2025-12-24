using System.Buffers;
using MessagePack;
using Microsoft.Extensions.Caching.Hybrid;

namespace Issue9SQLiteInt32.HybridCache.Serializers;

public class CustomMessagePackSerializer<T> : IHybridCacheSerializer<T>
{
    public T Deserialize(ReadOnlySequence<byte> source)
        => MessagePackSerializer.Deserialize<T>(source, CustomMessagePackSerializerOptions.SerializerOptions);

    public void Serialize(T value, IBufferWriter<byte> target)
        => MessagePackSerializer.Serialize(target, value, CustomMessagePackSerializerOptions.SerializerOptions);
}