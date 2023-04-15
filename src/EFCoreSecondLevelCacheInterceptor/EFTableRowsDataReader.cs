﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
#if NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
using System.Text.Json;
#endif

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Converts an EFTableRows to a DbDataReader.
/// </summary>
public class EFTableRowsDataReader : DbDataReader
{
    private static readonly Type LongType = typeof(long);
    private static readonly Type UlongTYpe = typeof(ulong);
    private static readonly Type BoolType = typeof(bool);
    private static readonly Type ByteType = typeof(byte);
    private static readonly Type StringType = typeof(string);
    private static readonly Type DateTimeType = typeof(DateTime);
    private static readonly Type DecimalType = typeof(decimal);
    private static readonly Type DoubleType = typeof(double);
    private static readonly Type FloatType = typeof(float);
    private static readonly Type ByteArrayType = typeof(byte[]);
    private static readonly Type ShortType = typeof(short);
    private static readonly Type IntType = typeof(int);
    private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);
    private static readonly Type TimeSpanType = typeof(TimeSpan);
    private static readonly Type UintType = typeof(uint);
    private static readonly Type CharType = typeof(char);

    private readonly int _rowsCount;
    private readonly EFTableRows _tableRows;

    private readonly Dictionary<int, Type> _valueTypes;

    private int _currentRow;

    private bool _isClosed;

    private IList<object> _rowValues = new List<object>();

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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == LongType)
        {
            return (long)value != 0;
        }

        if (valueType == UlongTYpe)
        {
            return (ulong)value != 0;
        }

        if (valueType != BoolType)
        {
            return (ulong)Convert.ChangeType(value, UlongTYpe, CultureInfo.InvariantCulture) != 0;
        }

        return (bool)value;
    }

    private Type getOrdinalValueType(int ordinal, object value)
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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == BoolType)
        {
            return (bool)value ? (byte)1 : (byte)0;
        }

        if (valueType == LongType)
        {
            return (byte)(long)value;
        }

        if (valueType != ByteType)
        {
            return (byte)Convert.ChangeType(value, ByteType, CultureInfo.InvariantCulture);
        }

        return (byte)value;
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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == StringType)
        {
            var val = value.ToString();
            if (string.IsNullOrWhiteSpace(val))
            {
                return default;
            }

            if (val.Length == 1)
            {
                return val[0];
            }

            return checked((char)GetInt64(ordinal));
        }

        return (char)value;
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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType != DateTimeType)
        {
            var s = value.ToString();
            return string.IsNullOrWhiteSpace(s)
                       ? default
                       : DateTime.Parse(s, CultureInfo.CurrentCulture);
        }

        return (DateTime)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a Decimal object.
    /// </summary>
    public override decimal GetDecimal(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == StringType)
        {
            var s = value.ToString();
            return string.IsNullOrWhiteSpace(s)
                       ? default
                       : decimal.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent,
                                       CultureInfo.InvariantCulture);
        }

        if (valueType != DecimalType)
        {
            return (decimal)Convert.ChangeType(value, DecimalType, CultureInfo.InvariantCulture);
        }

        return (decimal)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a double-precision floating point number.
    /// </summary>
    public override double GetDouble(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType != DoubleType)
        {
            return (double)Convert.ChangeType(value, DoubleType, CultureInfo.InvariantCulture);
        }

        return (double)value;
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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == DoubleType)
        {
            return (float)(double)value;
        }

        if (valueType != FloatType)
        {
            return (float)Convert.ChangeType(value, FloatType, CultureInfo.InvariantCulture);
        }

        return (float)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a globally unique identifier (GUID).
    /// </summary>
    public override Guid GetGuid(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return Guid.NewGuid();
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == StringType)
        {
            var g = value.ToString();
            return string.IsNullOrWhiteSpace(g) ? Guid.NewGuid() : new Guid(g);
        }

        if (valueType == ByteArrayType)
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
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == BoolType)
        {
            return (bool)value ? (short)1 : (short)0;
        }

        if (valueType == LongType)
        {
            return (short)(long)value;
        }

        if (valueType != ShortType)
        {
            return (short)Convert.ChangeType(value, ShortType, CultureInfo.InvariantCulture);
        }

        return (short)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a 32-bit signed integer.
    /// </summary>
    public override int GetInt32(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == BoolType)
        {
            return (bool)value ? 1 : 0;
        }

        if (valueType == LongType)
        {
            return (int)(long)value;
        }

        if (valueType != IntType)
        {
            return (int)Convert.ChangeType(value, IntType, CultureInfo.InvariantCulture);
        }

        return (int)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a 64-bit signed integer.
    /// </summary>
    public override long GetInt64(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return default;
        }

        var valueType = getOrdinalValueType(ordinal, value);
        if (valueType == BoolType)
        {
            return (bool)value ? 1 : 0;
        }

        if (valueType != LongType)
        {
            return (long)Convert.ChangeType(value, LongType, CultureInfo.InvariantCulture);
        }

        return (long)value;
    }

    /// <summary>
    ///     Gets the value of the specified column as a string.
    /// </summary>
    public override string GetString(int ordinal)
    {
        var value = GetValue(ordinal);
        if (IsNull(value))
        {
            return string.Empty;
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    ///     Gets the value of the specified column in its native format.
    /// </summary>
    public override object GetValue(int ordinal) => _rowValues[ordinal];


    /// <inheritdoc />
    public override T GetFieldValue<T>(int ordinal)
    {
        var value = GetValue(ordinal);
        var actualValueType = getOrdinalValueType(ordinal, value);
        var expectedValueType = typeof(T);

        if (expectedValueType == actualValueType)
        {
            return (T)value;
        }

        if (expectedValueType == DateTimeOffsetType && actualValueType == DateTimeType)
        {
            return (T)(object)new DateTimeOffset((DateTime)value);
        }

        if (expectedValueType == DateTimeOffsetType && actualValueType == StringType)
        {
            return (T)(object)DateTimeOffset.Parse((string)value, CultureInfo.CurrentCulture);
        }

        if (expectedValueType == TimeSpanType && actualValueType == StringType)
        {
            return (T)(object)TimeSpan.Parse((string)value, CultureInfo.CurrentCulture);
        }

        if (expectedValueType == TimeSpanType && IsNumber(actualValueType))
        {
            return (T)(object)new TimeSpan((ulong)value);
        }

        if (IsNumber(expectedValueType) && IsNumber(actualValueType))
        {
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        if (actualValueType == BoolType && IsNumber(expectedValueType))
        {
            return (bool)value ? (T)(object)1 : (T)(object)0;
        }

        if (expectedValueType == BoolType && IsNumber(actualValueType))
        {
            return (T)(object)((ulong)value != 0);
        }

        if (actualValueType.IsArray && expectedValueType.GetInterface(nameof(IEnumerable)) != null)
        {
            var enumerable = Activator.CreateInstance(typeof(T), value);
            if (enumerable is not null)
            {
                return (T)enumerable;
            }
        }

#if NET8_0 || NET7_0 || NET6_0 || NET5_0 || NETCORE3_1
        var dbTypeName = GetDataTypeName(ordinal);

        if (actualValueType == StringType && string.Equals(dbTypeName, "jsonb", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Deserialize<T>((string)value)!;
        }
#endif

#if NET8_0 || NET7_0 || NET6_0
        if (expectedValueType == DateOnlyType && actualValueType == DateTimeType)
        {
            return (T)(object)DateOnly.FromDateTime((DateTime)value);
        }

        if (expectedValueType == TimeOnlyType && actualValueType == TimeSpanType)
        {
            return (T)(object)TimeOnly.FromTimeSpan((TimeSpan)value);
        }
#endif

        return (T)value;
    }

    private static bool IsNumber(Type type) =>
        type == UintType || type == IntType ||
        type == UlongTYpe || type == LongType ||
        type == ShortType || type == ByteType || type == CharType;

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
        return IsNull(value);
    }

    private static bool IsNull(object? value) => value is null || value is DBNull;
#if NET8_0 || NET7_0 || NET6_0
    private static readonly Type DateOnlyType = typeof(DateOnly);
    private static readonly Type TimeOnlyType = typeof(TimeOnly);
#endif
}