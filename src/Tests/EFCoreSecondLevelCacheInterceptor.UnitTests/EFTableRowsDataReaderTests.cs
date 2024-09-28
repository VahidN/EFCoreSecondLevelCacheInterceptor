using System.Globalization;
using System.Text.Json;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFTableRowsDataReaderTests
{
    [Fact]
    public void GetFieldCount_ReturnsExpectedFieldCount()
    {
        // Arrange
        var tableRows = new EFTableRows { FieldCount = 1 };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.FieldCount;

        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void GetHasRows_ReturnsTrue_WhenRowsArePresent()
    {
        // Arrange
        var values = new List<object> { new() };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.HasRows;

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetHasRows_ReturnsFalse_WhenNoRowsArePresent()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.HasRows;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetIsClosed_ReturnsTrue_WhenDataReaderIsClosed()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        dataReader.Close();

        // Assert
        var actual = dataReader.IsClosed;

        Assert.True(actual);
    }

    [Fact]
    public void GetIsClosed_ReturnsFalse_WhenDataReaderIsOpen()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.IsClosed;

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetDepth_ReturnsExpectedDepth()
    {
        // Arrange
        var values = new List<object> { new() };
        var tableRow = new EFTableRow(values) { Depth = 1 };
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.Depth;

        // Assert
        Assert.Equal(1, actual);
    }

    [Fact]
    public void GetRecordsAffected_ReturnsMinusOne()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.RecordsAffected;

        // Assert
        Assert.Equal(-1, actual);
    }

    [Fact]
    public void GetTableName_ReturnsExpectedTableName()
    {
        // Arrange
        var tableRows = new EFTableRows { TableName = "TestTable" };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.TableName;

        // Assert
        Assert.Equal("TestTable", actual);
    }

    [Fact]
    public void GetTableName_ReturnsGuid()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = Guid.TryParse(dataReader.TableName, out _);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Indexer_ReturnsExpectedValue_WhenNameIsValid()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = "test", Ordinal = 1 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader["test"];

        // Assert
        Assert.Equal("test", actual);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenNameIsInvalid()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        dataReader.Read();

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader["invalid"]);
    }

    [Fact]
    public void Indexer_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object> { null };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = "null", Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader["null"];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void IndexerByOrdinal_ReturnsExpectedValue_WhenOrdinalIsValid()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader[1];

        // Assert
        Assert.Equal("test", actual);
    }

    [Fact]
    public void IndexerByOrdinal_ThrowsArgumentOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader[5]);
    }

    [Fact]
    public void IndexerByOrdinal_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object> { null };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader[0];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetDataTypeName_ReturnsExpectedTypeName_WhenOrdinalIsValid()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = "Int32" } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetDataTypeName(0);

        // Assert
        Assert.Equal("Int32", actual);
    }

    [Fact]
    public void GetDataTypeName_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetDataTypeName(5));
    }

    [Fact]
    public void GetDataTypeName_ReturnsEmptyString_WhenTypeNameIsEmpty()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = string.Empty } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetDataTypeName(0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetFieldType_ReturnsCorrectType_WhenOrdinalIsValid()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { TypeName = typeof(int).ToString() } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetFieldType(0);

        // Assert
        Assert.Equal(typeof(int), actual);
    }

    [Fact]
    public void GetFieldType_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act && Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetFieldType(5));
    }

    [Fact]
    public void GetFieldType_ReturnsNull_WhenFieldTypeIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { TypeName = string.Empty } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetFieldType(0);

        // Assert
        Assert.Equal(typeof(string), actual);
    }

    [Fact]
    public void GetName_ReturnsExpectedName_WhenOrdinalIsValid()
    {
        // Arrange
        var expected = Guid.NewGuid().ToString();
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = expected } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetName(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetName_ThrowsKeyNotFoundException_WhenOrdinalNotFound()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => dataReader.GetName(5));
    }

    [Fact]
    public void GetName_ReturnsEmptyString_WhenNameIsEmpty()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = string.Empty } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetName(0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetSchemaTable_ThrowsInvalidOperationException()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act && Assert
        Assert.Throws<InvalidOperationException>(() => dataReader.GetSchemaTable());
    }

    [Fact]
    public void NextResult_ReturnsFalse()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.NextResult();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Close_SetsIsClosedToTrue()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        dataReader.Close();

        // Assert
        Assert.True(dataReader.IsClosed);
    }

    [Fact]
    public void Read_ReturnsTrue_WhenCurrentRowIsWithinRange()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.Read();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Read_ReturnsFalse_WhenCurrentRowIsOutOfRange()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.Read();

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Read_UpdatesRowValues_WhenCurrentRowIsWithinRange()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        dataReader.Read();

        // Assert
        var actual = dataReader.GetValue(0);

        Assert.Equal(123, actual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(1L, true)]
    [InlineData(0L, false)]
    [InlineData(1UL, true)]
    [InlineData(0UL, false)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    public void GetBoolean_ReturnsExpectedValue(object value, bool expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetBoolean(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetByteData =>
        new List<object[]>
        {
            new object[] { null, default(byte) },
            new object[] { true, (byte)1 },
            new object[] { false, (byte)0 },
            new object[] { 123L, (byte)123 },
            new object[] { 123f, (byte)123 },
            new object[] { 123M, (byte)123 },
            new object[] { "123", (byte)123 },
            new object[] { (byte)123, (byte)123 }
        };

    [Theory]
    [MemberData(nameof(GetByteData))]
    public void GetByte_ReturnsExpectedValue(object value, byte expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetByte(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetByte_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object> { string.Empty };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetByte(0));
    }

    [Fact]
    public void GetByte_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object> { " " };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetByte(0));
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetBytes(0, 0, null, 0, 0);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new byte[10];

        // Act
        var actual = dataReader.GetBytes(0, 0, buffer, 0, buffer.Length);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetBytes_ReturnsZero_WhenDataOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new byte[10];

        // Act
        var actual = dataReader.GetBytes(0, 5, buffer, 0, buffer.Length);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Theory]
    [InlineData(null, default(char))]
    [InlineData("", default(char))]
    [InlineData(" ", default(char))]
    [InlineData("A", 'A')]
    [InlineData("65", 'A')]
    [InlineData(65L, 'A')]
    [InlineData(65, 'A')]
    public void GetChar_ReturnsExpectedValue(object value, char expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetChar(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act
        var actual = dataReader.GetChars(0, 0, null, 0, 0);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferIsNotNull()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(0, 0, buffer, 0, buffer.Length);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenDataOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(0, 5, buffer, 0, buffer.Length);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenBufferOffsetIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(0, 0, buffer, 5, buffer.Length - 5);

        // Assert
        Assert.Equal(0L, actual);
    }

    [Fact]
    public void GetChars_ReturnsZero_WhenLengthIsNonZero()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);
        var buffer = new char[10];

        // Act
        var actual = dataReader.GetChars(0, 0, buffer, 0, 5);

        // Assert
        Assert.Equal(0L, actual);
    }

    public static IEnumerable<object[]> GetDateTimeData =>
        new List<object[]>
        {
            new object[] { null, default(DateTime) },
            new object[] { string.Empty, default(DateTime) },
            new object[] { "2023-10-01T12:00:00", new DateTime(2023, 10, 1, 12, 0, 0) },
            new object[] { new DateTime(2023, 10, 1, 12, 0, 0), new DateTime(2023, 10, 1, 12, 0, 0) },
            new object[] { " ", default(DateTime) },
        };

    [Theory]
    [MemberData(nameof(GetDateTimeData))]
    public void GetDateTime_ReturnsExpectedValue(object value, DateTime expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetDateTime(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetDecimalData =>
        new List<object[]>
        {
            new object[] { null, default(decimal) },
            new object[] { string.Empty, default(decimal) },
            new object[] { "123.45", 123.45m },
            new object[] { 123.45m, 123.45m },
            new object[] { 123.45f, 123.45m },
            new object[] { 123.45, 123.45m },
            new object[] { " ", default(decimal) },
        };

    [Theory]
    [MemberData(nameof(GetDecimalData))]
    public void GetDecimal_ReturnsExpectedValue(object value, decimal expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetDecimal(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetDoubleData =>
        new List<object[]>
        {
            new object[] { null, default(double) },
            new object[] { "123.45", 123.45 },
            new object[] { 123.45, 123.45 }
        };

    [Theory]
    [MemberData(nameof(GetDoubleData))]
    public void GetDouble_ReturnsExpectedValue(object value, double expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetDouble(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDouble_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object> { string.Empty };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetDouble(0));
    }

    [Fact]
    public void GetDouble_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object> { " " };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetDouble(0));
    }

    [Fact]
    public void GetEnumerator_ThrowsNotSupportedException()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        // Act && Assert
        Assert.Throws<NotSupportedException>(() => dataReader.GetEnumerator());
    }

    public static IEnumerable<object[]> GetFloatData =>
        new List<object[]>
        {
            new object[] { null, default(float) },
            new object[] { "123.45", 123.45f },
            new object[] { 123.45, 123.45f },
            new object[] { 123.45f, 123.45f }
        };

    [Theory]
    [MemberData(nameof(GetFloatData))]
    public void GetFloat_ReturnsExpectedValue(object value, float expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFloat(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFloat_ThrowsFormatException_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object> { string.Empty };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetFloat(0));
    }

    [Fact]
    public void GetFloat_ThrowsFormatException_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object> { " " };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<FormatException>(() => dataReader.GetFloat(0));
    }

    public static IEnumerable<object[]> GetGuidData =>
        new List<object[]>
        {
            new object[] { "d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d", new Guid("d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d") },
            new object[]
            {
                new Guid("d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d"),
                new Guid("d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d")
            },
            new object[]
            {
                new Guid("d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d").ToByteArray(),
                new Guid("d3b07384-d9a7-4f3b-8a1b-3b2c1a1b2c3d")
            },
        };

    [Theory]
    [MemberData(nameof(GetGuidData))]
    public void GetGuid_ReturnsExpectedValue(object value, Guid expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object> { null };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object> { string.Empty };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    [Fact]
    public void GetGuid_ReturnsGuid_WhenValueIsWhiteSpace()
    {
        // Arrange
        var values = new List<object> { " " };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetGuid(0);

        // Assert
        Assert.IsType<Guid>(actual);
    }

    public static IEnumerable<object[]> GetInt16Data =>
        new List<object[]>
        {
            new object[] { null, default(short) },
            new object[] { true, (short)1 },
            new object[] { false, (short)0 },
            new object[] { (long)123, (short)123 },
            new object[] { 123, (short)123 },
            new object[] { (short)123, (short)123 }
        };

    [Theory]
    [MemberData(nameof(GetInt16Data))]
    public void GetInt16_ReturnsExpectedValue(object value, short expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt16(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetInt32Data =>
        new List<object[]>
        {
            new object[] { null, default(int) },
            new object[] { true, 1 },
            new object[] { false, 0 },
            new object[] { "123", 123 },
            new object[] { 123, 123 }
        };

    [Theory]
    [MemberData(nameof(GetInt32Data))]
    public void GetInt32_ReturnsExpectedValue(object value, int expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt32(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetInt64Data =>
        new List<object[]>
        {
            new object[] { null, default(long) },
            new object[] { true, 1L },
            new object[] { false, 0L },
            new object[] { 123L, 123L },
            new object[] { "123", 123L }
        };

    [Theory]
    [MemberData(nameof(GetInt64Data))]
    public void GetInt64_ReturnsExpectedValue(object value, long expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetInt64(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> GetStringData =>
        new List<object[]>
        {
            new object[] { null, string.Empty },
            new object[] { string.Empty, string.Empty },
            new object[] { 123.45, "123,45" }
        };

    [Theory]
    [MemberData(nameof(GetStringData))]
    public void GetString_ReturnsExpectedValue(object value, string expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetString(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValue_ReturnsExpectedValue_WhenOrdinalIsValid()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(1);

        // Assert
        Assert.Equal("test", actual);
    }

    [Fact]
    public void GetValue_ThrowsArgumentOutOfRangeException_WhenOrdinalIsOutOfRange()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act && Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => dataReader.GetValue(5));
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object> { null };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(0);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void GetValue_ReturnsEmptyString_WhenValueIsEmptyString()
    {
        // Arrange
        var values = new List<object> { string.Empty };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(0);

        // Assert
        Assert.Equal(string.Empty, actual);
    }

    [Fact]
    public void GetValue_ReturnsBoolean_WhenValueIsBoolean()
    {
        // Arrange
        var values = new List<object> { true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetValue(0);

        // Assert
        Assert.Equal(true, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedByteValue()
    {
        // Arrange
        const byte expected = 123;

        var values = new List<object> { expected };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<byte>(0);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValue()
    {
        // Arrange
        var values = new List<object> { true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<bool>(0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValue_WhenNumberIsULongType()
    {
        // Arrange
        var values = new List<object> { (ulong)1 };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<bool>(0);

        // Assert
        Assert.True(actual);
    }
    
    [Fact]
    public void GetFieldValue_ThrowsInvalidCastException_WhenNumberIsNotULongType()
    {
        // Arrange
        var values = new List<object> { (long)1 };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<bool>(0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedBoolValueFromString()
    {
        // Arrange
        var values = new List<object> { bool.TrueString };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(Boolean) } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<bool>(0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateTimeOffsetValueFromDateTime()
    {
        // Arrange
        var values = new List<object> { DateTime.MaxValue };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateTimeOffset>(0);

        // Assert
        Assert.Equal(DateTimeOffset.MaxValue.Date, actual.Date);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateTimeOffsetValueFromString()
    {
        // Arrange
        var values = new List<object> { DateTime.MaxValue.ToString(CultureInfo.InvariantCulture) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateTimeOffset>(0);

        // Assert
        Assert.Equal(DateTimeOffset.MaxValue.Date, actual.Date);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeSpanValueFromString()
    {
        // Arrange
        var values = new List<object> { TimeSpan.MaxValue.ToString() };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeSpan>(0);

        // Assert
        Assert.Equal(TimeSpan.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeSpanValueFromNumber()
    {
        // Arrange
        var values = new List<object> { TimeSpan.MaxValue.Ticks };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeSpan>(0);

        // Assert
        Assert.Equal(TimeSpan.MaxValue, actual);
    }

    public static IEnumerable<object[]> ValidNumberData =>
        new List<object[]>
        {
            new object[] { (sbyte)1 },
            new object[] { (byte)1 },
            new object[] { (short)1 },
            new object[] { (ushort)1 },
            new object[] { 1 },
            new object[] { 1U },
            new object[] { 1L },
            new object[] { 1UL },
            new object[] { 1F },
            new object[] { 1D },
            new object[] { 1M }
        };

    [Theory]
    [MemberData(nameof(ValidNumberData))]
    public void GetFieldValue_ShouldReturnExpectedDecimalNumber(object value)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = value.GetType().Name, Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<decimal>(0);

        // Assert
        Assert.Equal(1M, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedByteNumberFromChar()
    {
        // Arrange
        var values = new List<object> { '1' };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(Char), Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<byte>(0);

        // Assert
        Assert.Equal(49, actual);
    }

    [Theory]
    [MemberData(nameof(ValidNumberData))]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenConvertToDecimal(object value)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = value.GetType().Name, Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var exception = Record.Exception(() => dataReader.GetFieldValue<decimal>(0));

        // Assert
        Assert.Null(exception);
    }

    public static IEnumerable<object[]> InvalidNumberData =>
        new List<object[]>
        {
            new object[] { (nint)1 },
            new object[] { (nuint)1 },
            new object[] { "1" }
        };

    [Theory]
    [MemberData(nameof(InvalidNumberData))]
    public void GetFieldValue_ThrowsInvalidCastException_WhenValueIsNotNumber(object value)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = value.GetType().Name, Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        void Act() => dataReader.GetFieldValue<decimal>(0);

        // Assert
        Assert.Throws<InvalidCastException>(Act);
    }
    
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void GetFieldValue_ShouldReturnExpectedNumberValueFromBool(bool value, int expected)
    {
        // Arrange
        var values = new List<object> { value };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<int>(0);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetFieldValue_ShouldReturnExpectedArray()
    {
        // Arrange
        var values = new List<object> { new object[] { 1, 2, 3 } };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<int[]>(0);

        // Assert
        Assert.Equal(new [] { 1, 2, 3 }, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedList()
    {
        // Arrange
        var values = new List<object> { new [] { 1, 2, 3 } };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<List<int>>(0);

        // Assert
        Assert.Equal(new List<int> { 1, 2, 3 }, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedObject()
    {
        // Arrange
        var values = new List<object> { "{}" };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = "jsonb", Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = JsonSerializer.Serialize(dataReader.GetFieldValue<object>(0));

        // Assert
        Assert.Equal("{}", actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateOnlyValueFromDateTime()
    {
        // Arrange
        var values = new List<object> { DateTime.MaxValue };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(DateTime), Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateOnly>(0);

        // Assert
        Assert.Equal(DateOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedDateOnlyValueFromString()
    {
        // Arrange
        var values = new List<object> { DateOnly.MaxValue.ToString(CultureInfo.InvariantCulture) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(String), Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<DateOnly>(0);

        // Assert
        Assert.Equal(DateOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenValueConversionFromStringToDateOnly()
    {
        // Arrange
        var values = new List<object> { DateOnly.MaxValue.ToString(CultureInfo.InvariantCulture) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(String), Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var exception = Record.Exception(() => dataReader.GetFieldValue<DateOnly>(0));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromTimeSpan()
    {
        // Arrange
        var values = new List<object> { new TimeSpan(863999999999) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(TimeSpan), Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromNumber()
    {
        // Arrange
        var values = new List<object> { 863999999999 };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(TimeSpan), Ordinal = 0 } }
            }
        };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldReturnExpectedTimeOnlyValueFromString()
    {
        // Arrange
        var values = new List<object> { TimeOnly.MaxValue.ToString("O", CultureInfo.InvariantCulture) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(String), Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.GetFieldValue<TimeOnly>(0);

        // Assert
        Assert.Equal(TimeOnly.MaxValue, actual);
    }

    [Fact]
    public void GetFieldValue_ShouldNotThrowInvalidCastExceptionWhenValueConversionFromStringToTimeOnly()
    {
        // Arrange
        var values = new List<object> { TimeOnly.MaxValue.ToString(CultureInfo.InvariantCulture) };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows
        {
            Rows = new List<EFTableRow> { tableRow },
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { DbTypeName = nameof(String), Ordinal = 0 } }
            }
        };

        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var exception = Record.Exception(() => dataReader.GetFieldValue<TimeOnly>(0));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void GetValues_CopiesRowValuesToArray()
    {
        // Arrange
        var expected = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(expected);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();


        // Act
        var actual = new object[expected.Count];

        dataReader.GetValues(actual);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetValues_ReturnsRowValuesCount()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();


        // Act
        var actual = dataReader.GetValues(new object[values.Count]);

        // Assert
        Assert.Equal(values.Count, actual);
    }

    [Fact]
    public void GetValues_CopiesEmptyRowValuesToArray()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = Array.Empty<object>();

        dataReader.GetValues(actual);

        // Assert
        Assert.Empty(actual);
    }

    [Fact]
    public void GetValues_CopiesPartialRowValuesToArray()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = new object[values.Count + 1];

        dataReader.GetValues(actual);

        // Assert
        Assert.Equal(actual.Take(values.Count), values);
    }

    [Fact]
    public void IsDBNull_ReturnsTrue_WhenValueIsNull()
    {
        // Arrange
        var values = new List<object> { null };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.IsDBNull(0);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsDBNull_ReturnsFalse_WhenValueIsNotNull()
    {
        // Arrange
        var values = new List<object> { 123, "test", true };
        var tableRow = new EFTableRow(values);
        var tableRows = new EFTableRows { Rows = new List<EFTableRow> { tableRow } };
        var dataReader = new EFTableRowsDataReader(tableRows);

        dataReader.Read();

        // Act
        var actual = dataReader.IsDBNull(0);

        // Assert
        Assert.False(actual);
    }
}