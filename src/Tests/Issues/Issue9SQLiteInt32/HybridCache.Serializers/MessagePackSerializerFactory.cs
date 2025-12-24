using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;

namespace Issue9SQLiteInt32.HybridCache.Serializers;

public class MessagePackSerializerFactory : IHybridCacheSerializerFactory
{
    public bool TryCreateSerializer<T>([NotNullWhen(returnValue: true)] out IHybridCacheSerializer<T>? serializer)
    {
        serializer = new CustomMessagePackSerializer<T>();

        return true;
    }

    protected virtual bool SupportsType<T>() => true;

}