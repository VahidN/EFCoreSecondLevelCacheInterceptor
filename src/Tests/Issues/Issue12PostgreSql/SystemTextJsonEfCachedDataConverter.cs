using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EFCoreSecondLevelCacheInterceptor;

namespace Issue12PostgreSql;

public class SystemTextJsonEfCachedDataConverter : JsonConverter<EFCachedData>
{
    public override EFCachedData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        var root = doc.RootElement;
        var cachedData = new EFCachedData();

        if (root.TryGetProperty(nameof(EFCachedData.TableRows), out var tableRowsElement) &&
            tableRowsElement.ValueKind != JsonValueKind.Null)
        {
            cachedData.TableRows = DeserializeEFTableRows(tableRowsElement);
        }

        if (root.TryGetProperty(nameof(EFCachedData.NonQuery), out var nonQueryElement) &&
            nonQueryElement.ValueKind != JsonValueKind.Null)
        {
            cachedData.NonQuery = nonQueryElement.TryGetInt32(out var nonQueryValue) ? nonQueryValue : 0;
        }

        if (root.TryGetProperty(nameof(EFCachedData.Scalar), out var scalarElement) &&
            scalarElement.ValueKind != JsonValueKind.Null)
        {
            cachedData.Scalar = scalarElement.ValueKind == JsonValueKind.Null
                ? null
                : JsonSerializer.Deserialize<object>(scalarElement.GetRawText(), options);
        }

        if (root.TryGetProperty(nameof(EFCachedData.IsNull), out var isNullElement) &&
            isNullElement.ValueKind != JsonValueKind.Null)
        {
            cachedData.IsNull = isNullElement.ValueKind == JsonValueKind.True;
        }

        return cachedData;
    }

    private static EFTableRows DeserializeEFTableRows(JsonElement tableRowsElement)
    {
        var tableRows = new EFTableRows();

        if (tableRowsElement.TryGetProperty(nameof(EFTableRows.ColumnsInfo), out var columnsInfoElement) &&
            columnsInfoElement.ValueKind == JsonValueKind.Object)
        {
            using var objectEnumerator = columnsInfoElement.EnumerateObject();

            foreach (var columnProp in objectEnumerator)
            {
                if (int.TryParse(columnProp.Name, CultureInfo.InvariantCulture, out var ordinal))
                {
                    tableRows.ColumnsInfo[ordinal] = DeserializeEFTableColumnInfo(columnProp.Value);
                }
            }
        }

        if (tableRowsElement.TryGetProperty(nameof(EFTableRows.Rows), out var rowsElement) &&
            rowsElement.ValueKind == JsonValueKind.Array)
        {
            using var arrayEnumerator = rowsElement.EnumerateArray();

            foreach (var rowElement in arrayEnumerator)
            {
                var efTableRow = DeserializeEFTableRow(rowElement, tableRows);

                if (efTableRow != null)
                {
                    tableRows.Add(efTableRow);
                }
            }
        }

        if (tableRowsElement.TryGetProperty(nameof(EFTableRows.FieldCount), out var fieldCountElement))
        {
            tableRows.FieldCount = fieldCountElement.TryGetInt32(out var fieldCount) ? fieldCount : 0;
        }

        if (tableRowsElement.TryGetProperty(nameof(EFTableRows.TableName), out var tableNameElement))
        {
            tableRows.TableName = tableNameElement.GetString() ?? Guid.NewGuid().ToString();
        }

        if (tableRowsElement.TryGetProperty(nameof(EFTableRows.VisibleFieldCount), out var visibleFieldCountElement))
        {
            tableRows.VisibleFieldCount = visibleFieldCountElement.TryGetInt32(out var visibleCount) ? visibleCount : 0;
        }

        return tableRows;
    }

    private static EFTableRow? DeserializeEFTableRow(JsonElement rowElement, EFTableRows tableRows)
    {
        if (rowElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var values = new List<object>();

        if (rowElement.TryGetProperty(nameof(EFTableRow.Values), out var valuesElement) &&
            valuesElement.ValueKind == JsonValueKind.Array)
        {
            using var arrayEnumerator = valuesElement.EnumerateArray();

            var i = 0;

            foreach (var valueElement in arrayEnumerator)
            {
                var column = tableRows.ColumnsInfo[i];
                var item = ReadValue(valueElement, column.TypeName) ?? DBNull.Value;
                values.Add(item);
                i++;
            }
        }

        var efTableRow = new EFTableRow(values);

        if (rowElement.TryGetProperty(nameof(EFTableRow.Depth), out var depthElement))
        {
            efTableRow.Depth = depthElement.TryGetInt32(out var depth) ? depth : 0;
        }

        return efTableRow;
    }

    private static EFTableColumnInfo DeserializeEFTableColumnInfo(JsonElement columnInfoElement)
    {
        var columnInfo = new EFTableColumnInfo();

        if (columnInfoElement.TryGetProperty(nameof(EFTableColumnInfo.Ordinal), out var ordinalElement))
        {
            columnInfo.Ordinal = ordinalElement.TryGetInt32(out var ordinal) ? ordinal : 0;
        }

        if (columnInfoElement.TryGetProperty(nameof(EFTableColumnInfo.Name), out var nameElement))
        {
            columnInfo.Name = nameElement.GetString() ?? string.Empty;
        }

        if (columnInfoElement.TryGetProperty(nameof(EFTableColumnInfo.DbTypeName), out var dbTypeNameElement))
        {
            columnInfo.DbTypeName = dbTypeNameElement.GetString() ?? typeof(string).ToString();
        }

        if (columnInfoElement.TryGetProperty(nameof(EFTableColumnInfo.TypeName), out var typeNameElement))
        {
            columnInfo.TypeName = typeNameElement.GetString() ?? typeof(string).ToString();
        }

        return columnInfo;
    }

    private static object? ReadValue(JsonElement element, string typeName)
        => element.ValueKind == JsonValueKind.Null
            ? null
            : typeName switch
            {
                "System.String" => element.GetString(),

                "System.Int32" => element.GetInt32(),

                "System.Int16" => element.GetInt16(),

                "System.Int64" => element.GetInt64(),

                "System.Boolean" => element.GetBoolean(),

                "System.Single" => element.GetSingle(),

                "System.Double" => element.GetDouble(),

                "System.Decimal" => element.GetDecimal(),

                "System.Guid" => element.GetGuid(),

                "System.DateTime" => element.GetDateTime(),

                "System.DateTimeOffset" => element.GetDateTimeOffset(),

                "System.DateOnly" => DateOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture),

                "System.TimeOnly" => TimeOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture),

                "System.TimeSpan" => TimeSpan.Parse(element.GetString()!, CultureInfo.InvariantCulture),

                "System.Byte[]" => element.GetBytesFromBase64(),

                _ => JsonSerializer.Deserialize(element.GetRawText(), Type.GetType(typeName)!)
            };

    public override void Write(Utf8JsonWriter writer, EFCachedData? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();

            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName(propertyName: "TableRows");

        if (value.TableRows == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.TableRows, options);
        }

        writer.WritePropertyName(propertyName: "NonQuery");
        writer.WriteNumberValue(value.NonQuery);

        writer.WritePropertyName(propertyName: "Scalar");

        if (value.Scalar == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Scalar, options);
        }

        writer.WritePropertyName(propertyName: "IsNull");
        writer.WriteBooleanValue(value.IsNull);

        writer.WriteEndObject();
    }
}