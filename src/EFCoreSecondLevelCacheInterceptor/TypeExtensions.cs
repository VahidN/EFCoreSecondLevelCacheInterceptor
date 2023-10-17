using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Type Helper utilities
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Cached version of typeof(long)
    /// </summary>
    public static readonly Type LongType = typeof(long);

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
    public static bool IsNull(this object? value) => value is null || value is DBNull;

    /// <summary>
    ///     IsGenericType or IsArray
    /// </summary>
    /// <param name="expectedValueType"></param>
    /// <returns></returns>
    public static bool IsArrayOrGenericList(Type? expectedValueType) =>
        (expectedValueType?.IsGenericType == true && expectedValueType.GetGenericTypeDefinition() == typeof(List<>)) ||
        expectedValueType?.IsArray == true;

    /// <summary>
    ///     Is it a number type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsNumber(Type type) =>
        type == UintType || type == IntType ||
        type == UlongTYpe || type == LongType ||
        type == ShortType || type == ByteType || type == CharType;

#if NET8_0 || NET7_0 || NET6_0
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