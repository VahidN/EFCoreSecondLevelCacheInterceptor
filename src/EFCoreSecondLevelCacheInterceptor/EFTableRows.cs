using System.Data.Common;
using System.Runtime.Serialization;
using System.Text;

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
            "boolean[]" or "bool[]" => typeof(bool[]).FullName!,
            "smallint[]" or "int2[]" => typeof(short[]).FullName!,
            "integer[]" or "int4[]" => typeof(int[]).FullName!,
            "bigint[]" or "int8[]" => typeof(long[]).FullName!,
            "real[]" or "float4[]" => typeof(float[]).FullName!,
            "double precision[]" or "float8[]" => typeof(double[]).FullName!,
            "numeric[]" => typeof(decimal[]).FullName!,
            "money[]" => typeof(decimal[]).FullName!,
            "text[]" => typeof(string[]).FullName!,
            "character varying[]" or "varchar[]" => typeof(string[]).FullName!,
            "character[]" or "bpchar[]" => typeof(string[]).FullName!,
            "citext[]" => typeof(string[]).FullName!,
            "name[]" => typeof(string[]).FullName!,
            "json[]" => typeof(string[]).FullName!,
            "jsonb[]" => typeof(string[]).FullName!,
            "jsonpath[]" => typeof(string[]).FullName!,
            "xml[]" => typeof(string[]).FullName!,
            "bit[]" or "bit varying[]" or "varbit[]" => typeof(System.Collections.BitArray[]).FullName!,
            "\"char\"[]" => typeof(char[]).FullName!,
            "uuid[]" => typeof(Guid[]).FullName!,
            "date[]" => typeof(DateTime[]).FullName!,
            "timestamp[]" or "timestamp without time zone[]" => typeof(DateTime[]).FullName!,
            "timestamptz[]" or "timestamp with time zone[]" => typeof(DateTime[]).FullName!,
            "time[]" or "time without time zone[]" => typeof(TimeSpan[]).FullName!,
            "timetz[]" or "time with time zone[]" => typeof(DateTimeOffset[]).FullName!,
            "interval[]" => typeof(TimeSpan[]).FullName!,
            "oid[]" => typeof(uint[]).FullName!,
            "xid[]" => typeof(uint[]).FullName!,
            "cid[]" => typeof(uint[]).FullName!,
            "bytea" => typeof(byte[]).FullName!,
            "bytea[]" => typeof(byte[][]).FullName!,
            _ => throw new NotSupportedException(dbTypeName)
        };

    // Remove type modifiers such as (50) while preserving parentheses inside quoted identifiers.
    private static string RemovePgTypeModifiers(string dbTypeName)
    {
        var result = new StringBuilder(dbTypeName.Length);
        var modifierDepth = 0;
        var insideQuotedIdentifier = false;

        for (var i = 0; i < dbTypeName.Length; i++)
        {
            var character = dbTypeName[i];

            if (character == '"')
            {
                if (modifierDepth == 0)
                {
                    result.Append(character);
                }

                if (insideQuotedIdentifier && i + 1 < dbTypeName.Length && dbTypeName[i + 1] == '"')
                {
                    if (modifierDepth == 0)
                    {
                        result.Append(dbTypeName[++i]);
                    }
                    else
                    {
                        i++;
                    }

                    continue;
                }

                insideQuotedIdentifier = !insideQuotedIdentifier;
                continue;
            }

            if (!insideQuotedIdentifier && character == '(')
            {
                modifierDepth++;
                continue;
            }

            if (!insideQuotedIdentifier && character == ')' && modifierDepth > 0)
            {
                modifierDepth--;
                continue;
            }

            if (modifierDepth == 0)
            {
                result.Append(character);
            }
        }

        // Avoid silently changing malformed type names with an unmatched modifier or quote.
        return modifierDepth == 0 && !insideQuotedIdentifier ? result.ToString() : dbTypeName;
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