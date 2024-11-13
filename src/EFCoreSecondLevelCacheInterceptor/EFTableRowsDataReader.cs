using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
#if NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
using System.Text.Json;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Converts an EFTableRows to a DbDataReader.
/// </summary>
public class EFTableRowsDataReader : DbDataReader
{
    private readonly int _rowsCount;
    private readonly EFTableRows _tableRows;
    private readonly Dictionary<int, Type> _valueTypes;
    private int _currentRow;
    private bool _isClosed;
    private IList<object> _rowValues = [];

    /// <summary>
    ///     Converts an EFTableRows to a DbDataReader.
    /// </summary>
    public EFTableRowsDataReader(EFTableRows tableRows)
    {
        _tableRows = tableRows;
        _rowsCount = _tableRows.RowsCount;
        _valueTypes = new Dictionary<int, Type>(_tableRows.FieldCount);
    }

    /// <summary>
    ///     Gets the number of columns in the current row.
    /// </summary>
    public override int FieldCount => _tableRows.FieldCount;

    /// <summary>
    ///     Gets a value that indicates whether the SqlDataReader contains one or more rows.
    /// </summary>
    public override bool HasRows => _rowsCount > 0;

    /// <summary>
    ///     Retrieves a Boolean value that indicates whether the specified SqlDataReader instance has been closed.
    /// </summary>
    public override bool IsClosed => _isClosed;

    /// <summary>
    ///     Gets a value that indicates the depth of nesting for the current row.
    /// </summary>
    public override int Depth => _tableRows.Get(_currentRow)?.Depth ?? 0;

    /// <summary>
    ///     Gets the number of rows changed, inserted, or deleted by execution of the Transact-SQL statement.
    /// </summary>
    public override int RecordsAffected => -1;

    /// <summary>
    ///     The TableName's unique ID.
    /// </summary>
    public string TableName => _tableRows.TableName;

    /// <summary>
    ///     Returns GetValue(GetOrdinal(name))
    /// </summary>
    public override object this[string name] => GetValue(GetOrdinal(name));

    /// <summary>
    ///     Returns GetValue(ordinal)
    /// </summary>
    public override object this[int ordinal] => GetValue(ordinal);

    /// <summary>
    ///     Gets a string representing the data type of the specified column.
    /// </summary>
    public override string GetDataTypeName(int ordinal) => _tableRows.GetDataTypeName(ordinal);

    /// <summary>
    ///     Gets the Type that is the data type of the object.
    /// </summary>
    public override Type GetFieldType(int ordinal) => _tableRows.GetFieldType(ordinal);

    /// <summary>
    ///     Gets the name of the specified column.
    /// </summary>
    public override string GetName(int ordinal) => _tableRows.GetName(ordinal);

    /// <summary>
    ///     Gets the column ordinal, given the name of the column.
    /// </summary>
    public override int GetOrdinal(string name) => _tableRows.GetOrdinal(name);

    /// <summary>
    ///     Returns a DataTable that describes the column metadata of the SqlDataReader.
    /// </summary>
    public override DataTable GetSchemaTable() => throw new InvalidOperationException();

    /// <summary>
    ///     Advances the data reader to the next result, when reading the results of batch Transact-SQL statements.
    /// </summary>
    public override bool NextResult() => false;

    /// <summary>
    ///     Closes the SqlDataReader object.
    /// </summary>
    public override void Close() => _isClosed = true;

    /// <summary>
    ///     Advances the SqlDataReader to the next record.
    /// </summary>
    public override bool Read()
    {
        if (_currentRow >= _rowsCount)
        {
            return false;
        }

        _rowValues = _tableRows.Get(_currentRow++).Values;

        return true;
    }

    /// <summary>
    ///     Gets the value of the specified column as a Boolean.
    /// </summary>
    public override bool GetBoolean(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.LongType)
        {
            return (long)value != 0;
        }

        if (valueType == TypeExtensions.UlongTYpe)
        {
            return (ulong)value != 0;
        }

        if (valueType != TypeExtensions.BoolType)
        {
            return (ulong)Convert.ChangeType(value, TypeExtensions.UlongTYpe, CultureInfo.InvariantCulture) != 0;
        }

        return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    }

    private Type GetOrdinalValueType(int ordinal, object value)
    {
        if (_valueTypes.TryGetValue(ordinal, out var valueType))
        {
            return valueType;
        }

        valueType = value.GetType();
        _valueTypes.Add(ordinal, valueType);

        return valueType;
    }

    /// <summary>
    ///     Gets the value of the specified column as a byte.
    /// </summary>
    public override byte GetByte(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.BoolType)
        {
            return (bool)value ? (byte)1 : (byte)0;
        }

        if (valueType == TypeExtensions.LongType)
        {
            return (byte)(long)value;
        }

        if (valueType != TypeExtensions.ByteType)
        {
            return (byte)Convert.ChangeType(value, TypeExtensions.ByteType, CultureInfo.InvariantCulture);
        }

        return Convert.ToByte(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Reads a stream of bytes from the specified column offset into the buffer an array starting at the given buffer
    ///     offset.
    /// </summary>
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => 0L;

    /// <summary>
    ///     Gets the value of the specified column as a single character.
    /// </summary>
    public override char GetChar(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.StringType)
        {
            var val = value.ToString();

            if (string.IsNullOrWhiteSpace(val))
            {
                return default;
            }

            if (val.Length == 1)
            {
                return val[index: 0];
            }

            return checked((char)GetInt64(ordinal));
        }

        return Convert.ToChar(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Reads a stream of characters from the specified column offset into the buffer as an array starting at the given
    ///     buffer offset.
    /// </summary>
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => 0L;

    /// <summary>
    ///     Gets the value of the specified column as a DateTime object.
    /// </summary>
    public override DateTime GetDateTime(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType != TypeExtensions.DateTimeType)
        {
            var s = value.ToString();

            return string.IsNullOrWhiteSpace(s) ? default : DateTime.Parse(s, CultureInfo.InvariantCulture);
        }

        return (DateTime)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a Decimal object.
    /// </summary>
    public override decimal GetDecimal(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.StringType)
        {
            var s = value.ToString();

            return string.IsNullOrWhiteSpace(s)
                ? default
                : decimal.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
        }

        if (valueType != TypeExtensions.DecimalType)
        {
            return (decimal)Convert.ChangeType(value, TypeExtensions.DecimalType, CultureInfo.InvariantCulture);
        }

        return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the value of the specified column as a double-precision floating point number.
    /// </summary>
    public override double GetDouble(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType != TypeExtensions.DoubleType)
        {
            return (double)Convert.ChangeType(value, TypeExtensions.DoubleType, CultureInfo.InvariantCulture);
        }

        return Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Returns an IEnumerator that iterates through the SqlDataReader.
    /// </summary>
    public override IEnumerator GetEnumerator() => throw new NotSupportedException();

    /// <summary>
    ///     Gets the value of the specified column as a single-precision floating point number.
    /// </summary>
    public override float GetFloat(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.DoubleType)
        {
            return (float)(double)value;
        }

        if (valueType != TypeExtensions.FloatType)
        {
            return (float)Convert.ChangeType(value, TypeExtensions.FloatType, CultureInfo.InvariantCulture);
        }

        return Convert.ToSingle(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the value of the specified column as a globally unique identifier (GUID).
    /// </summary>
    public override Guid GetGuid(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return Guid.NewGuid();
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.StringType)
        {
            var g = value.ToString();

            return string.IsNullOrWhiteSpace(g) ? Guid.NewGuid() : new Guid(g);
        }

        if (valueType == TypeExtensions.ByteArrayType)
        {
            return new Guid((byte[])value);
        }

        return (Guid)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a 16-bit signed integer.
    /// </summary>
    public override short GetInt16(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.BoolType)
        {
            return (bool)value ? (short)1 : (short)0;
        }

        if (valueType == TypeExtensions.LongType)
        {
            return (short)(long)value;
        }

        if (valueType != TypeExtensions.ShortType)
        {
            return (short)Convert.ChangeType(value, TypeExtensions.ShortType, CultureInfo.InvariantCulture);
        }

        return Convert.ToInt16(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the value of the specified column as a 32-bit signed integer.
    /// </summary>
    public override int GetInt32(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.BoolType)
        {
            return (bool)value ? 1 : 0;
        }

        if (valueType == TypeExtensions.LongType)
        {
            return (int)(long)value;
        }

        if (valueType != TypeExtensions.IntType)
        {
            return (int)Convert.ChangeType(value, TypeExtensions.IntType, CultureInfo.InvariantCulture);
        }

        return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the value of the specified column as a 64-bit signed integer.
    /// </summary>
    public override long GetInt64(int ordinal)
    {
        var value = GetValue(ordinal);

        if (value.IsNull())
        {
            return default;
        }

        var valueType = GetOrdinalValueType(ordinal, value);

        if (valueType == TypeExtensions.BoolType)
        {
            return (bool)value ? 1 : 0;
        }

        if (valueType != TypeExtensions.LongType)
        {
            return (long)Convert.ChangeType(value, TypeExtensions.LongType, CultureInfo.InvariantCulture);
        }

        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Gets the value of the specified column as a string.
    /// </summary>
    public override string GetString(int ordinal)
    {
        var value = GetValue(ordinal);

        return value.IsNull() ? string.Empty : value.ToString() ?? string.Empty;
    }

    /// <summary>
    ///     Gets the value of the specified column in its native format.
    /// </summary>
    public override object GetValue(int ordinal) => _rowValues[ordinal];

    /// <inheritdoc />
    public override T GetFieldValue<T>(int ordinal)
    {
        var value = GetValue(ordinal);
        var actualValueType = GetOrdinalValueType(ordinal, value);
        var expectedValueType = typeof(T);

        if (expectedValueType == actualValueType)
        {
            return (T)value;
        }

        if (expectedValueType == TypeExtensions.DateTimeOffsetType && actualValueType == TypeExtensions.DateTimeType)
        {
            return (T)(object)new DateTimeOffset((DateTime)value);
        }

        if (expectedValueType == TypeExtensions.DateTimeOffsetType && actualValueType == TypeExtensions.StringType)
        {
            return (T)(object)DateTimeOffset.Parse((string)value, CultureInfo.InvariantCulture);
        }

        if (expectedValueType == TypeExtensions.TimeSpanType && actualValueType == TypeExtensions.StringType)
        {
            return (T)(object)TimeSpan.Parse((string)value, CultureInfo.InvariantCulture);
        }

        var isActualValueTypeNumber = TypeExtensions.IsNumber(actualValueType);

        if (expectedValueType == TypeExtensions.TimeSpanType && isActualValueTypeNumber)
        {
            return (T)(object)new TimeSpan(Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }

        var isExpectedValueTypeNumber = TypeExtensions.IsNumber(expectedValueType);

        if (isExpectedValueTypeNumber && isActualValueTypeNumber)
        {
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        if (actualValueType == TypeExtensions.BoolType && isExpectedValueTypeNumber)
        {
            return (bool)value ? (T)(object)1 : (T)(object)0;
        }

        if (expectedValueType == TypeExtensions.BoolType && isActualValueTypeNumber)
        {
            return (T)(object)((ulong)value != 0);
        }

        if (actualValueType.IsArray && TypeExtensions.IsArrayOrGenericList(expectedValueType) &&
            value is IEnumerable enumerable)
        {
            return ProcessPostgresArrayOrList<T>(expectedValueType, enumerable);
        }

#if NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
        var dbTypeName = GetDataTypeName(ordinal);

        if (actualValueType == TypeExtensions.StringType &&
            string.Equals(dbTypeName, b: "jsonb", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Deserialize<T>((string)value)!;
        }
#endif

#if NET9_0 || NET8_0 || NET7_0 || NET6_0
        if (expectedValueType == TypeExtensions.DateOnlyType && actualValueType == TypeExtensions.DateTimeType)
        {
            return (T)(object)DateOnly.FromDateTime((DateTime)value);
        }

        if (expectedValueType == TypeExtensions.DateOnlyType && actualValueType == TypeExtensions.StringType)
        {
            return (T)(object)DateOnly.Parse((string)value, CultureInfo.InvariantCulture);
        }

        if (expectedValueType == TypeExtensions.TimeOnlyType && actualValueType == TypeExtensions.TimeSpanType)
        {
            return (T)(object)TimeOnly.FromTimeSpan((TimeSpan)value);
        }

        if (expectedValueType == TypeExtensions.TimeOnlyType && actualValueType == TypeExtensions.StringType)
        {
            return (T)(object)TimeOnly.Parse((string)value, CultureInfo.InvariantCulture);
        }

        if (expectedValueType == TypeExtensions.TimeOnlyType && isActualValueTypeNumber)
        {
            return (T)(object)TimeOnly.FromTimeSpan(new TimeSpan(Convert.ToInt64(value, CultureInfo.InvariantCulture)));
        }
#endif

        return (T)value;
    }

    private static T ProcessPostgresArrayOrList<T>(Type expectedValueType, IEnumerable enumerable)
    {
        var elementType = expectedValueType.IsArray
            ? expectedValueType.GetElementType()
            : expectedValueType.GetGenericArguments()[0];

        if (elementType is null)
        {
            throw new InvalidOperationException(
                $"Expected ValueType `{nameof(elementType)}` must be an array or a generic list.");
        }

        var items = enumerable.OfType<object>().ToArray();
        var array = Array.CreateInstance(elementType, items.Length);

        for (var i = 0; i < items.Length; i++)
        {
            array.SetValue(items[i], i);
        }

        if (expectedValueType.IsArray)
        {
            return (T)(object)array;
        }

        var type = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(type)!;

        foreach (var item in items)
        {
            list.Add(item);
        }

        return (T)list;
    }

    /// <summary>
    ///     Populates an array of objects with the column values of the current row.
    /// </summary>
    public override int GetValues(object[] values)
    {
        Array.Copy(_rowValues.ToArray(), values, _rowValues.Count);

        return _rowValues.Count;
    }

    /// <summary>
    ///     Gets a value that indicates whether the column contains non-existent or missing values.
    /// </summary>
    public override bool IsDBNull(int ordinal)
    {
        var value = _rowValues[ordinal];

        return value.IsNull();
    }
}