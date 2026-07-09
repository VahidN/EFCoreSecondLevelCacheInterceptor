namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]

// ReSharper disable once InconsistentNaming
public class EFTableColumnInfoTests
{
    [TestMethod]
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
        Assert.AreEqual(expected: "Ordinal: 1, Name: ColumnName, DbTypeName: DbType, TypeName= Type.", actual);
    }

    [TestMethod]
    public void ToString_ReturnsFormattedString_WithDefaultProperties()
    {
        // Arrange
        var columnInfo = new EFTableColumnInfo();

        // Act
        var actual = columnInfo.ToString();

        // Assert
        Assert.AreEqual(expected: "Ordinal: 0, Name: , DbTypeName: , TypeName= .", actual);
    }
}