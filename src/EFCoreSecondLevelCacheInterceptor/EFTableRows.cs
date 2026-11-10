using System.Data.Common;
using System.Runtime.Serialization;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     TableRows structure
/// </summary>
[Serializable]
[DataContract]
public class EFTableRows
{
    /// <summary>
    ///     TableRows structure
    /// </summary>
    public EFTableRows(DbDataReader reader)
    {
        if (reader == null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        ColumnsInfo = new Dictionary<int, EFTableColumnInfo>(reader.FieldCount);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var dbTypeName = reader.GetDataTypeName(i) ?? typeof(string).ToString();
            var typeName = reader.GetFieldType(i)?.ToString() ?? typeof(string).ToString();

            if (string.Equals(typeName, typeof(Array).FullName, StringComparison.Ordinal))
            {
                typeName = GetPgTypeArrayFullName(dbTypeName);
            }

            ColumnsInfo.Add(i, new EFTableColumnInfo
            {
                Ordinal = i,
                Name = reader.GetName(i),
                DbTypeName = dbTypeName,
                TypeName = typeName
            });
        }
    }

    /// <summary>
    ///     TableRows structure
    /// </summary>
    public EFTableRows() => ColumnsInfo = new Dictionary<int, EFTableColumnInfo>();

    /// <summary>
    ///     Rows of the table
    /// </summary>
    [DataMember]
    public IList<EFTableRow> Rows { get; set; } = [];

    /// <summary>
    ///     TableColumn's Info
    /// </summary>
    [DataMember]
    public IDictionary<int, EFTableColumnInfo> ColumnsInfo { get; set; }

    /// <summary>
    ///     Gets the number of columns in the current row.
    /// </summary>
    [DataMember]
    public int FieldCount { get; set; }

    /// <summary>
    ///     EFTableRows's unique ID
    /// </summary>
    [DataMember]
    public string TableName { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     Gets a value that indicates whether the SqlDataReader contains one or more rows.
    /// </summary>
    [DataMember]
    public bool HasRows => Rows?.Count > 0;

    /// <summary>
    ///     Gets the number of rows changed, inserted, or deleted by execution of the Transact-SQL statement.
    /// </summary>
    [DataMember]
    public int RecordsAffected { get; } = -1;

    /// <summary>
    ///     Gets the number of fields in the SqlDataReader that are not hidden.
    /// </summary>
    [DataMember]
    public int VisibleFieldCount { get; set; }

    /// <summary>
    ///     Number of Db rows.
    /// </summary>
    [DataMember]
    public int RowsCount => Rows?.Count ?? 0;

    /// <summary>
    ///     Gets or sets the Get(index)
    /// </summary>
    public EFTableRow this[int index]
    {
        get => Get(index);
        set => Rows[index] = value;
    }

    private static string GetPgTypeArrayFullName(string dbTypeName)
        => RemovePgTypeModifiers(dbTypeName) switch
        {
            "boolean[]" => typeof(bool[]).FullName!,
            "smallint[]" => typeof(short[]).FullName!,
            "integer[]" => typeof(int[]).FullName!,
            "bigint[]" => typeof(long[]).FullName!,
            "real[]" => typeof(float[]).FullName!,
            "double precision[]" => typeof(double[]).FullName!,
            "numeric[]" => typeof(decimal[]).FullName!,
            "money[]" => typeof(decimal[]).FullName!,
            "text[]" => typeof(string[]).FullName!,
            "character varying[]" => typeof(string[]).FullName!,
            "character[]" => typeof(string[]).FullName!,
            "citext[]" => typeof(string[]).FullName!,
            "name[]" => typeof(string[]).FullName!,
            "json[]" => typeof(string[]).FullName!,
            "jsonb[]" => typeof(string[]).FullName!,
            "jsonpath[]" => typeof(string[]).FullName!,
            "xml[]" => typeof(string[]).FullName!,
            "\"char\"[]" => typeof(char[]).FullName!,
            "uuid[]" => typeof(Guid[]).FullName!,
            "date[]" => typeof(DateTime[]).FullName!,
            "timestamp without time zone[]" => typeof(DateTime[]).FullName!,
            "timestamp with time zone[]" => typeof(DateTime[]).FullName!,
            "time without time zone[]" => typeof(TimeSpan[]).FullName!,
            "time with time zone[]" => typeof(DateTimeOffset[]).FullName!,
            "interval[]" => typeof(TimeSpan[]).FullName!,
            "oid[]" => typeof(uint[]).FullName!,
            "xid[]" => typeof(uint[]).FullName!,
            "cid[]" => typeof(uint[]).FullName!,
            "bytea" => typeof(byte[]).FullName!,
            "bytea[]" => typeof(byte[][]).FullName!,
            _ => throw new NotSupportedException(dbTypeName)
        };

    // Npgsql includes facets in type names, e.g. "character varying(50)[]" or "timestamp(3) with time zone[]".
    private static string RemovePgTypeModifiers(string dbTypeName)
    {
        var start = dbTypeName.IndexOf(value: "(", StringComparison.Ordinal);

        if (start < 0)
        {
            return dbTypeName;
        }

        var end = dbTypeName.IndexOf(value: ")", start, StringComparison.Ordinal);

        return end < 0 ? dbTypeName : dbTypeName.Remove(start, end - start + 1);
    }

    /// <summary>
    ///     Adds an item to the EFTableRows
    /// </summary>
    public void Add(EFTableRow item)
    {
        if (item != null)
        {
            Rows.Add(item);
        }
    }

    /// <summary>
    ///     returns the value of the given index.
    /// </summary>
    public EFTableRow Get(int index) => Rows[index];

    /// <summary>
    ///     Gets the column ordinal, given the name of the column.
    /// </summary>
    public int GetOrdinal(string name)
    {
        var keyValuePair =
            ColumnsInfo.FirstOrDefault(pair => pair.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (keyValuePair.Value != null)
        {
            return keyValuePair.Value.Ordinal;
        }

        throw new ArgumentOutOfRangeException(nameof(name), name);
    }

    /// <summary>
    ///     Gets the name of the specified column.
    /// </summary>
    public string GetName(int ordinal) => GetColumnInfo(ordinal).Name;

    /// <summary>
    ///     Gets a string representing the data type of the specified column.
    /// </summary>
    public string GetDataTypeName(int ordinal) => GetColumnInfo(ordinal).DbTypeName;

    /// <summary>
    ///     Gets the Type that is the data type of the object.
    /// </summary>
    public Type GetFieldType(int ordinal) => Type.GetType(GetColumnInfo(ordinal).TypeName) ?? typeof(string);

    /// <summary>
    ///     Gets the Type that is the data type of the object.
    /// </summary>
    public string GetFieldTypeName(int ordinal) => GetColumnInfo(ordinal).TypeName;

    private EFTableColumnInfo GetColumnInfo(int ordinal)
    {
        var dbColumnInfo = ColumnsInfo[ordinal];

        if (dbColumnInfo != null)
        {
            return dbColumnInfo;
        }

        throw new ArgumentOutOfRangeException(nameof(ordinal), $"Index[{ordinal}] was outside of array's bounds.");
    }
}