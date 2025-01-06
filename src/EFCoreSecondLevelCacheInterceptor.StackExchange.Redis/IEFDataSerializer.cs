namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     High-Level API of a Serializer.
/// </summary>
public interface IEFDataSerializer
{
    /// <summary>
    ///     Serializes a given value with the specified buffer writer.
    /// </summary>
    byte[] Serialize<T>(T? obj);

    /// <summary>
    ///     Deserializes a value of a given type from a sequence of bytes.
    /// </summary>
    T? Deserialize<T>(byte[]? data);
}