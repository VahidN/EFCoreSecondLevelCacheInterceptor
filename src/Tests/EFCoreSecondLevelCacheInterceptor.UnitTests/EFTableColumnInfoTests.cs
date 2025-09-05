using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFTableColumnInfoTests
{
    [Fact]
    public void ToString_ReturnsFormattedString_WithValidProperties()
    {
        // Arrange
        var columnInfo = new EFTableColumnInfo
        {
            Ordinal = 1,
            Name = "ColumnName",
            DbTypeName = "DbType",
            TypeName = "Type"
        };

        // Act
        var actual = columnInfo.ToString();

        // Assert
        Assert.Equal(expected: "Ordinal: 1, Name: ColumnName, DbTypeName: DbType, TypeName= Type.", actual);
    }

    [Fact]
    public void ToString_ReturnsFormattedString_WithDefaultProperties()
    {
        // Arrange
        var columnInfo = new EFTableColumnInfo();

        // Act
        var actual = columnInfo.ToString();

        // Assert
        Assert.Equal(expected: "Ordinal: 0, Name: , DbTypeName: , TypeName= .", actual);
    }
}