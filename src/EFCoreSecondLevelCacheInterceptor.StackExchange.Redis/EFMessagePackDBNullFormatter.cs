using System;
using MessagePack;
using MessagePack.Formatters;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     The contract for serialization of some specific type.
/// </summary>
public class EFMessagePackDBNullFormatter : IMessagePackFormatter<DBNull?>
{
    /// <summary>
    ///     DBNullFormatter instance
    /// </summary>
    public static readonly EFMessagePackDBNullFormatter Instance = new();

    private EFMessagePackDBNullFormatter()
    {
    }

    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, DBNull? value, MessagePackSerializerOptions options)
        => writer.WriteNil();

    /// <inheritdoc />
    public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => DBNull.Value;
}