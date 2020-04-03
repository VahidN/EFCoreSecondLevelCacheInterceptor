using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// TableRows structure
    /// </summary>
    [Serializable]
    public class EFTableRows
    {
        /// <summary>
        /// Rows of the table
        /// </summary>
        public List<EFTableRow> Rows { set; get; } = new List<EFTableRow>();

        /// <summary>
        /// TableColumn's Info
        /// </summary>
        public Dictionary<int, EFTableColumnInfo> ColumnsInfo { set; get; }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// EFTableRows's unique ID
        /// </summary>
        public string TableName { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets a value that indicates whether the SqlDataReader contains one or more rows.
        /// </summary>
        public bool HasRows => Rows?.Count > 0;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the Transact-SQL statement.
        /// </summary>
        public int RecordsAffected => -1;

        /// <summary>
        /// Gets the number of fields in the SqlDataReader that are not hidden.
        /// </summary>
        public int VisibleFieldCount { get; set; }

        /// <summary>
        /// Number of Db rows.
        /// </summary>
        public int RowsCount => Rows?.Count ?? 0;

        /// <summary>
        /// Gets or sets the Get(index)
        /// </summary>
        public EFTableRow this[int index]
        {
            get
            {
                return Get(index);
            }
            set
            {
                Rows[index] = value;
            }
        }

        /// <summary>
        /// TableRows structure
        /// </summary>
        public EFTableRows(DbDataReader reader)
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>(reader.FieldCount);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                ColumnsInfo.Add(i, new EFTableColumnInfo
                {
                    Ordinal = i,
                    Name = reader.GetName(i),
                    DbTypeName = reader.GetDataTypeName(i),
                    TypeName = reader.GetFieldType(i).ToString()
                });
            }
        }

        /// <summary>
        /// TableRows structure
        /// </summary>
        public EFTableRows()
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>();
        }

        /// <summary>
        /// Adds an item to the EFTableRows
        /// </summary>
        public void Add(EFTableRow item)
        {
            if (item != null)
            {
                Rows.Add(item);
            }
        }

        /// <summary>
        /// returns the value of the given index.
        /// </summary>
        public EFTableRow Get(int index) => Rows[index];

        /// <summary>
        /// Gets the column ordinal, given the name of the column.
        /// </summary>
        public int GetOrdinal(string name)
        {
            var keyValuePair = ColumnsInfo.FirstOrDefault(pair => pair.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (keyValuePair.Value != null)
            {
                return keyValuePair.Value.Ordinal;
            }
            throw new IndexOutOfRangeException(name);
        }

        /// <summary>
        /// Gets the name of the specified column.
        /// </summary>
        public string GetName(int ordinal) => getColumnInfo(ordinal).Name;

        /// <summary>
        /// Gets a string representing the data type of the specified column.
        /// </summary>
        public string GetDataTypeName(int ordinal) => getColumnInfo(ordinal).DbTypeName;

        /// <summary>
        /// Gets the Type that is the data type of the object.
        /// </summary>
        public Type GetFieldType(int ordinal) => Type.GetType(getColumnInfo(ordinal).TypeName);

        /// <summary>
        /// Gets the Type that is the data type of the object.
        /// </summary>
        public string GetFieldTypeName(int ordinal) => getColumnInfo(ordinal).TypeName;

        private EFTableColumnInfo getColumnInfo(int ordinal)
        {
            var dbColumnInfo = ColumnsInfo[ordinal];
            if (dbColumnInfo != null)
            {
                return dbColumnInfo;
            }
            throw new IndexOutOfRangeException($"Index[{ordinal}] was outside of array's bounds.");
        }
    }
}