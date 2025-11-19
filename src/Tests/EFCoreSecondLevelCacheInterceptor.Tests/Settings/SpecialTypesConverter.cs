using Newtonsoft.Json;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public class SpecialTypesConverter : JsonConverter
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override bool CanConvert(Type objectType)
        => objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?) || objectType == typeof(DateTime) ||
           objectType == typeof(DateTime?) || objectType == typeof(DateTimeOffset) ||
           objectType == typeof(DateTimeOffset?);

    public override object? ReadJson(JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
        => reader.Value;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(name: "$type"); // Deserializer helper
        writer.WriteValue(value?.GetType().FullName);
        writer.WritePropertyName(name: "$value");
        writer.WriteValue(value);
        writer.WriteEndObject();
    }
}