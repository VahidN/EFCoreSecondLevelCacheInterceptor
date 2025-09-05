using System.Data.Common;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFTableRowsTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenReaderIsNull()
    {
        // Arrange
        DbDataReader reader = null;

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(paramName: "reader", () => new EFTableRows(reader));
    }

    [Fact]
    public void Constructor_InitializesColumnsInfo_WithValidReader()
    {
        // Arrange
        var readerMock = new Mock<DbDataReader>();

        readerMock.Setup(r => r.FieldCount).Returns(value: 2);
        readerMock.Setup(r => r.GetName(0)).Returns(value: "Column1");
        readerMock.Setup(r => r.GetName(1)).Returns(value: "Column2");
        readerMock.Setup(r => r.GetDataTypeName(0)).Returns(value: "int");
        readerMock.Setup(r => r.GetDataTypeName(1)).Returns(value: "string");
        readerMock.Setup(r => r.GetFieldType(0)).Returns(typeof(int));
        readerMock.Setup(r => r.GetFieldType(1)).Returns(typeof(string));

        // Act
        var tableRows = new EFTableRows(readerMock.Object);

        // Assert
        Assert.Equal(expected: 2, tableRows.ColumnsInfo.Count);
        Assert.Equal(expected: "Column1", tableRows.ColumnsInfo[key: 0].Name);
        Assert.Equal(expected: "Column2", tableRows.ColumnsInfo[key: 1].Name);
        Assert.Equal(expected: "int", tableRows.ColumnsInfo[key: 0].DbTypeName);
        Assert.Equal(expected: "string", tableRows.ColumnsInfo[key: 1].DbTypeName);
        Assert.Equal(typeof(int).ToString(), tableRows.ColumnsInfo[key: 0].TypeName);
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[key: 1].TypeName);
    }

    [Fact]
    public void Constructor_SetsDefaultTypeName_WhenFieldTypeIsNull()
    {
        // Arrange
        var readerMock = new Mock<DbDataReader>();

        readerMock.Setup(r => r.FieldCount).Returns(value: 1);
        readerMock.Setup(r => r.GetName(0)).Returns(value: "Column1");
        readerMock.Setup(r => r.GetDataTypeName(0)).Returns((string)null);
        readerMock.Setup(r => r.GetFieldType(0)).Returns((Type)null);

        // Act
        var tableRows = new EFTableRows(readerMock.Object);

        // Assert
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[key: 0].DbTypeName);
        Assert.Equal(typeof(string).ToString(), tableRows.ColumnsInfo[key: 0].TypeName);
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
    public void Indexer_ReturnsExpectedRow_WhenIndexIsValid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object>
            {
                1
            }),
            new(new List<object>
            {
                2
            })
        };

        var tableRows = new EFTableRows
        {
            Rows = rows
        };

        // Act
        var actual = tableRows[index: 1];

        // Assert
        Assert.Equal(expected: 2, actual.Values[index: 0]);
    }

    [Fact]
    public void Indexer_SetsExpectedRow_WhenIndexIsValid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object>
            {
                1
            }),
            new(new List<object>
            {
                2
            })
        };

        var tableRows = new EFTableRows
        {
            Rows = rows
        };

        var newRow = new EFTableRow(new List<object>
        {
            3
        });

        // Act
        tableRows[index: 1] = newRow;

        // Assert
        Assert.Equal(expected: 3, tableRows[index: 1].Values[index: 0]);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenIndexIsInvalid()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object>
            {
                1
            })
        };

        var tableRows = new EFTableRows
        {
            Rows = rows
        };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => tableRows[index: 1]);
    }

    [Fact]
    public void Indexer_ThrowsArgumentOutOfRangeException_WhenSettingInvalidIndex()
    {
        // Arrange
        var rows = new List<EFTableRow>
        {
            new(new List<object>
            {
                1
            })
        };

        var tableRows = new EFTableRows
        {
            Rows = rows
        };

        var newRow = new EFTableRow(new List<object>
        {
            2
        });

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => tableRows[index: 1] = newRow);
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
    public void Get_ShouldReturnExpectedRow()
    {
        // Arrange
        var tableRows = new EFTableRows();
        var row = new EFTableRow(new List<object>());

        tableRows.Add(row);

        // Act
        var actual = tableRows.Get(index: 0);

        // Assert
        Assert.Equal(row, actual);
    }

    [Fact]
    public void GetOrdinal_ShouldReturnExpectedOrdinal()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = "Column1"
                    }
                }
            }
        };

        // Act
        var ordinal = tableRows.GetOrdinal(name: "Column1");

        // Assert
        Assert.Equal(expected: 0, ordinal);
    }

    [Fact]
    public void GetName_ShouldReturnExpectedName()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            ColumnsInfo = new Dictionary<int, EFTableColumnInfo>
            {
                {
                    0, new EFTableColumnInfo
                    {
                        Name = "Column1"
                    }
                }
            }
        };

        // Act
        var name = tableRows.GetName(ordinal: 0);

        // Assert
        Assert.Equal(expected: "Column1", name);
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
        Assert.Equal(expected: -1, tableRows.RecordsAffected);
    }

    [Fact]
    public void VisibleFieldCount_ReturnsExpectedValue_WhenSet()
    {
        // Arrange
        var tableRows = new EFTableRows
        {
            VisibleFieldCount = 5
        };

        // Act && Assert
        Assert.Equal(expected: 5, tableRows.VisibleFieldCount);
    }

    [Fact]
    public void VisibleFieldCount_DefaultsToZero_WhenNotSet()
    {
        var tableRows = new EFTableRows();

        Assert.Equal(expected: 0, tableRows.VisibleFieldCount);
    }

    [Fact]
    public void GetFieldTypeName_ReturnsTypeName_WhenOrdinalIsValid()
    {
        // Arrange
        var columnsInfo = new Dictionary<int, EFTableColumnInfo>
        {
            {
                0, new EFTableColumnInfo
                {
                    Ordinal = 0,
                    TypeName = "System.String"
                }
            }
        };

        var tableRows = new EFTableRows
        {
            ColumnsInfo = columnsInfo
        };

        // Act
        var actual = tableRows.GetFieldTypeName(ordinal: 0);

        Assert.Equal(expected: "System.String", actual);
    }

    [Fact]
    public void GetFieldTypeName_ThrowsArgumentOutOfRangeException_WhenOrdinalIsInvalid()
    {
        // Arrange
        var columnsInfo = new Dictionary<int, EFTableColumnInfo>
        {
            {
                0, null
            }
        };

        var tableRows = new EFTableRows
        {
            ColumnsInfo = columnsInfo
        };

        // Act
        var actual = Assert.Throws<ArgumentOutOfRangeException>(() => tableRows.GetFieldTypeName(ordinal: 0));

        // Assert
        Assert.Equal(expected: "Index[0] was outside of array's bounds. (Parameter 'ordinal')", actual.Message);
    }
}