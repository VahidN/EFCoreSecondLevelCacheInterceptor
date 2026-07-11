using System.Collections;
using System.Globalization;
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Type Helper utilities
/// </summary>
public static class TypeExtensions
{
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0
    /// <summary>
    ///     Cached version of typeof(JsonElement)
    /// </summary>
    public static readonly Type JsonElement = typeof(JsonElement);
#endif

    /// <summary>
    ///     Cached version of typeof(long)
    /// </summary>
    public static readonly Type LongType = typeof(long);

    /// <summary>
    ///     Cached version of typeof(Guid)
    /// </summary>
    public static readonly Type GuidType = typeof(Guid);

    /// <summary>
    ///     Cached version of typeof(ulong)
    /// </summary>
    public static readonly Type UlongTYpe = typeof(ulong);

    /// <summary>
    ///     Cached version of typeof(bool)
    /// </summary>
    public static readonly Type BoolType = typeof(bool);

    /// <summary>
    ///     Cached version of typeof(byte)
    /// </summary>
    public static readonly Type ByteType = typeof(byte);

    /// <summary>
    ///     Cached version of typeof(sbyte)
    /// </summary>
    public static readonly Type SByteType = typeof(sbyte);

    /// <summary>
    ///     Cached version of typeof(string)
    /// </summary>
    public static readonly Type StringType = typeof(string);

    /// <summary>
    ///     Cached version of typeof(DateTime)
    /// </summary>
    public static readonly Type DateTimeType = typeof(DateTime);

    /// <summary>
    ///     Cached version of typeof(decimal)
    /// </summary>
    public static readonly Type DecimalType = typeof(decimal);

    /// <summary>
    ///     Cached version of typeof(double)
    /// </summary>
    public static readonly Type DoubleType = typeof(double);

    /// <summary>
    ///     Cached version of typeof(float)
    /// </summary>
    public static readonly Type FloatType = typeof(float);

    /// <summary>
    ///     Cached version of typeof(byte[])
    /// </summary>
    public static readonly Type ByteArrayType = typeof(byte[]);

    /// <summary>
    ///     Cached version of typeof(short)
    /// </summary>
    public static readonly Type ShortType = typeof(short);

    /// <summary>
    ///     Cached version of typeof(ushort)
    /// </summary>
    public static readonly Type UShortType = typeof(ushort);

    /// <summary>
    ///     Cached version of typeof(int)
    /// </summary>
    public static readonly Type IntType = typeof(int);

    /// <summary>
    ///     Cached version of typeof(DateTimeOffset)
    /// </summary>
    public static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);

    /// <summary>
    ///     Cached version of typeof(TimeSpan)
    /// </summary>
    public static readonly Type TimeSpanType = typeof(TimeSpan);

    /// <summary>
    ///     Cached version of typeof(uint)
    /// </summary>
    public static readonly Type UintType = typeof(uint);

    /// <summary>
    ///     Cached version of typeof(char)
    /// </summary>
    public static readonly Type CharType = typeof(char);

    /// <summary>
    ///     Check value is DBNull
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNull(this object? value) => value is null or DBNull;

    /// <summary>
    ///     Check value is Null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsDbNullOrEmpty(
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
        [NotNullWhen(returnValue: false)]
#endif
        this string? value)
        => string.IsNullOrWhiteSpace(value) || value is "{}" or "[]" or "null";

    /// <summary>
    ///     IsGenericType or IsArray
    /// </summary>
    /// <param name="expectedValueType"></param>
    /// <returns></returns>
    public static bool IsArrayOrGenericList(Type? expectedValueType)
        => (expectedValueType?.IsGenericType == true &&
            expectedValueType.GetGenericTypeDefinition() == typeof(List<>)) || expectedValueType?.IsArray == true;

    /// <summary>
    ///     Is it a number type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNumber(Type type)
        => type == UintType || type == IntType || type == UlongTYpe || type == LongType || type == UShortType ||
           type == ShortType || type == SByteType || type == ByteType || type == FloatType || type == DoubleType ||
           type == DecimalType || type == CharType;

    /// <summary>
    ///     Converts an IEnumerable to a generic one
    /// </summary>
    public static T ConvertEnumerable<T>(this IEnumerable enumerable)
    {
        var targetCollectionType = typeof(T);

        var items = enumerable.Cast<object?>().ToArray();

        if (targetCollectionType.IsArray)
        {
            var targetElementType = targetCollectionType.GetCollectionElementType();
            var array = Array.CreateInstance(targetElementType, items.Length);

            for (var i = 0; i < items.Length; i++)
            {
                array.SetValue(items[i], i);
            }

            return (T)(object)array;
        }

        var list = (IList)Activator.CreateInstance(targetCollectionType)!;

        foreach (var item in items)
        {
            list.Add(item);
        }

        return (T)list;
    }

    /// <summary>
    ///     Returns the type of the collection
    /// </summary>
    public static Type GetCollectionElementType(this Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType() ??
                   throw new InvalidOperationException($"Unable to determine element type of '{collectionType}'.");
        }

        if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return collectionType.GetGenericArguments()[0];
        }

        throw new InvalidOperationException($"'{collectionType}' is not a supported collection type.");
    }

    /// <summary>
    ///     Returns a formatted value
    /// </summary>
    public static string FormatDbValue(this object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        if (value is string s)
        {
            return $"\"{s}\"";
        }

        if (value is IEnumerable enumerable and not string)
        {
            return "[" + string.Join(separator: ", ", enumerable.Cast<object?>().Select(FormatDbValue)) + "]";
        }

        return value.ToString() ?? value.GetType().Name;
    }

#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0
    /// <summary>
    ///     Returns the given JsonElement value
    /// </summary>
    public static T GetJsonElementValue<T>(this object value, JsonSerializerOptions? options)
    {
        if (value is not JsonElement element || element.IsJsonValueNullOrEmpty())
        {
            return default!;
        }

        var expectedType = typeof(T);

        switch (element.ValueKind)
        {
            case JsonValueKind.String when expectedType != StringType:
                var jsonString = element.GetString();

                if (!string.IsNullOrEmpty(jsonString) &&
                    (jsonString.StartsWith(value: '{') || jsonString.StartsWith(value: '[')))
                {
                    return options is null
                        ? JsonSerializer.Deserialize<T>(jsonString)!
                        : JsonSerializer.Deserialize<T>(jsonString, options)!;
                }

                break;
            case JsonValueKind.Object:
                return DeserializeFallback<T>(element, options);
        }

        object? converted = expectedType switch
        {
            _ when expectedType == StringType => element.GetString(),
            _ when expectedType == BoolType => element.GetBoolean(),
            _ when expectedType == ByteType => element.GetByte(),
            _ when expectedType == SByteType => element.GetSByte(),
            _ when expectedType == ShortType => element.GetInt16(),
            _ when expectedType == UShortType => element.GetUInt16(),
            _ when expectedType == IntType => element.GetInt32(),
            _ when expectedType == UintType => element.GetUInt32(),
            _ when expectedType == LongType => element.GetInt64(),
            _ when expectedType == UlongTYpe => element.GetUInt64(),
            _ when expectedType == FloatType => element.GetSingle(),
            _ when expectedType == DoubleType => element.GetDouble(),
            _ when expectedType == DecimalType => element.GetDecimal(),
            _ when expectedType == GuidType => element.GetGuid(),
            _ when expectedType == DateTimeType => element.GetDateTime(),
            _ when expectedType == DateTimeOffsetType => element.GetDateTimeOffset(),
            _ when expectedType == DateOnlyType => DateOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture),
            _ when expectedType == TimeOnlyType => TimeOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture),
            _ when expectedType == TimeSpanType => TimeSpan.Parse(element.GetString()!, CultureInfo.InvariantCulture),
            _ when expectedType == ByteArrayType => element.GetBytesFromBase64(),

            _ => DeserializeFallback<T>(element, options)
        };

        if (converted is null)
        {
            return default!;
        }

        return converted is T result
            ? result
            : throw new InvalidCastException(
                $"Unable to cast '{element.GetRawText()}' to '{expectedType.FullName}': '{converted.FormatDbValue()}'. ValueKind: '{element.ValueKind}'.");
    }

    private static T DeserializeFallback<T>(JsonElement element, JsonSerializerOptions? options)
    {
        var rawText = element.GetRawText().Trim();

        if (rawText.IsDbNullOrEmpty())
        {
            return default!;
        }

        return options is null
            ? JsonSerializer.Deserialize<T>(rawText)!
            : JsonSerializer.Deserialize<T>(rawText, options)!;
    }

    /// <summary>
    ///     Is it JsonValueKind.Null
    /// </summary>
    public static bool IsJsonValueNullOrEmpty(this JsonElement element)
        => element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;

    /// <summary>
    ///     Cached version of typeof(DateOnly)
    /// </summary>
    public static readonly Type DateOnlyType = typeof(DateOnly);

    /// <summary>
    ///     Cached version of typeof(TimeOnly)
    /// </summary>
    public static readonly Type TimeOnlyType = typeof(TimeOnly);
#endif
}