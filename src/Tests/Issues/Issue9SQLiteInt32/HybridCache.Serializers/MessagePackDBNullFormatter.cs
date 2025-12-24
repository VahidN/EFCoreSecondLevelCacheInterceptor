using MessagePack;
using MessagePack.Formatters;

namespace Issue9SQLiteInt32.HybridCache.Serializers;

/// <summary>
///     The contract for serialization of some specific type.
/// </summary>
public class MessagePackDBNullFormatter : IMessagePackFormatter<DBNull?>
{
    /// <summary>
    ///     DBNullFormatter instance
    /// </summary>
    public static readonly MessagePackDBNullFormatter Instance = new();

    private MessagePackDBNullFormatter()
    {
    }

    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, DBNull? value, MessagePackSerializerOptions options)
        => writer.WriteNil();

    /// <inheritdoc />
    public DBNull Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => DBNull.Value;
}