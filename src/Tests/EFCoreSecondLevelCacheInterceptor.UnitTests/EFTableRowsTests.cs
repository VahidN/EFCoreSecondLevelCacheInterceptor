using System.Data.Common;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFTableRowsTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenReaderIsNull()
    {
        // Arrange
        DbDataReader reader = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>("reader", () => new EFTableRows(reader));
    }

    [Fact]
    public void Constructor_InitializesColumnsInfo_WithValidReader()
    {
        // Arrange
        var readerMock = new Mock<DbDataReader>();

        readerMock.Setup(r => r.FieldCount).Returns(2);
        readerMock.Setup(r => r.GetName(0)).Returns("Column1");
        readerMock.Setup(r => r.GetName(1)).Returns("Column2");
        readerMock.Setup(r => r.GetDataTypeName(0)).Returns("int");
        readerMock.Setup(r => r.GetDataTypeName(1)).Returns("string");
        readerMock.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
        readerMock.Setup(r => r.GetFieldType(1)).Returns(typeof(string));

        // Act
        var tableRows = new EFTableRows(readerMock.Object);

        // Assert
        Assert.Equal(2, tableRows.ColumnsInfo.Count);
        Assert.Equal("Column1", tableRows.ColumnsInfo[0].Name);
        Assert.Equal("Column2", tableRows.ColumnsInfo[1].Name);
        Assert.Equal("int", tableRows.ColumnsInfo[0].DbTypeName);
        Assert.Equal("string", tableRows.ColumnsInfo[1].DbTypeName);
        Assert.Equal(typeof(int).ToString(), tableRows.ColumnsInfo[0].TypeName);
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[1].TypeName);
    }

    [Fact]
    public void Constructor_SetsDefaultTypeName_WhenFieldTypeIsNull()
    {
        // Arrange
        var readerMock = new Mock<DbDataReader>();

        readerMock.Setup(r => r.FieldCount).Returns(1);
        readerMock.Setup(r => r.GetName(0)).Returns("Column1");
        readerMock.Setup(r => r.GetDataTypeName(0)).Returns((string)null);
        readerMock.Setup(r => r.GetFieldType(0)).Returns((Type)null);

        // Act
        var tableRows = new EFTableRows(readerMock.Object);

        // Assert
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[0].DbTypeName);
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[0].TypeName);
    }

    [Fact]
    public void Constructor_ShouldInitializeColumnsInfo_WhenReaderIsValid()
    {
        // Arrange
        var reader = Mock.Of<DbDataReader>();

        // Act
        var tableRows = new EFTableRows(reader);

        // Assert
        Assert.Equal(reader.FieldCount, tableRows.ColumnsInfo.Count);
    }

    [Fact]
    public void Indexer_ReturnsCorrectRow_WhenIndexIsValid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object> { 1 }),
            new(new List<object> { 2 })
        };

        var tableRows = new EFTableRows { Rows = rows };

        // Act
        var result = tableRows[1];

        // Assert
        Assert.Equal(2, result.Values[0]);
    }

    [Fact]
    public void Indexer_SetsCorrectRow_WhenIndexIsValid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object> { 1 }),
            new(new List<object> { 2 })
        };

        var tableRows = new EFTableRows { Rows = rows };

        var newRow = new EFTableRow(new List<object> { 3 });

        // Act
        tableRows[1] = newRow;

        // Assert
        Assert.Equal(3, tableRows[1].Values[0]);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenIndexIsInvalid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object> { 1 })
        };

        var tableRows = new EFTableRows { Rows = rows };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => tableRows[1]);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenSettingInvalidIndex()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object> { 1 })
        };

        var tableRows = new EFTableRows { Rows = rows };
        var newRow = new EFTableRow(new List<object> { 2 });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => tableRows[1] = newRow);
    }

    [Fact]
    public void Add_ShouldAddItemToRows()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var row = new EFTableRow(new List<object>());

        // Act
        tableRows.Add(row);

        // Assert
        Assert.Single(tableRows.Rows);
    }

    [Fact]
    public void Get_ShouldReturnCorrectRow()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var row = new EFTableRow(new List<object>());

        tableRows.Add(row);

        // Act
        var result = tableRows.Get(0);

        // Assert
        Assert.Equal(row, result);
    }

    [Fact]
    public void GetOrdinal_ShouldReturnCorrectOrdinal()
    {
        // Arrange
        var tableRows = new EFTableRows()
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = "Column1" } }
            }
        };

        // Act
        var ordinal = tableRows.GetOrdinal("Column1");

        // Assert
        Assert.Equal(0, ordinal);
    }

    [Fact]
    public void GetName_ShouldReturnCorrectName()
    {
        // Arrange
        var tableRows = new EFTableRows()
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                { 0, new EFTableColumnInfo { Name = "Column1" } }
            }
        };

        // Act
        var name = tableRows.GetName(0);

        // Assert
        Assert.Equal("Column1", name);
    }

    [Fact]
    public void HasRows_ShouldReturnTrue_WhenRowsArePresent()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var row = new EFTableRow(new List<object>());

        tableRows.Add(row);

        // Act
        var hasRows = tableRows.HasRows;

        // Assert
        Assert.True(hasRows);
    }

    [Fact]
    public void RecordsAffected_ReturnsMinusOne()
    {
        // Arrange
        var tableRows = new EFTableRows();

        // Act && Assert
        Assert.Equal(-1, tableRows.RecordsAffected);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsCorrectValue_WhenSet()
    {
        // Arrange
        var tableRows = new EFTableRows { VisibleFieldCount = 5 };

        // Act && Assert
        Assert.Equal(5, tableRows.VisibleFieldCount);
    }

    [Fact]
    public void VisibleFieldCount_DefaultsToZero_WhenNotSet()
    {
        var tableRows = new EFTableRows();

        Assert.Equal(0, tableRows.VisibleFieldCount);
    }

    [Fact]
    public void GetFieldTypeName_ReturnsTypeName_WhenOrdinalIsValid()
    {
        // Arrange
        var columnsInfo = new Dictionary<int, EFTableColumnInfo>
        {
            { 0, new EFTableColumnInfo { Ordinal = 0, TypeName = "System.String" } }
        };

        var tableRows = new EFTableRows { ColumnsInfo = columnsInfo };

        // Act
        var result = tableRows.GetFieldTypeName(0);

        Assert.Equal("System.String", result);
    }

    [Fact]
    public void GetFieldTypeName_ThrowsArgumentOutOfRangeException_WhenOrdinalIsInvalid()
    {
        // Arrange
        var columnsInfo = new Dictionary<int, EFTableColumnInfo>
        {
            { 0, null }
        };

        var tableRows = new EFTableRows { ColumnsInfo = columnsInfo };

        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => tableRows.GetFieldTypeName(0));

        // Assert
        Assert.Equal("Index[0] was outside of array's bounds. (Parameter 'ordinal')", exception.Message);
    }
}