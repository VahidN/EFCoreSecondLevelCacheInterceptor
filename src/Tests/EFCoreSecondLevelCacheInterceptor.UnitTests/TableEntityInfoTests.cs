

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]
public class TableEntityInfoTests
{
    [TestMethod]
    public void ToString_ReturnsExpectedFormat_WhenClrTypeAndTableNameAreSet()
    {
        // Arrange
        var entityInfo = new TableEntityInfo
        {
            ClrType = typeof(string),
            TableName = "TestTable"
        };

        // Act
        var actual = entityInfo.ToString();

        // Assert
        Assert.AreEqual(expected: "System.String::TestTable", actual);
    }

    [TestMethod]
    public void ToString_ReturnsExpectedFormat_WhenClrTypeIsNull()
    {
        // Arrange
        var entityInfo = new TableEntityInfo
        {
            ClrType = null!,
            TableName = "TestTable"
        };

        // Act
        var actual = entityInfo.ToString();

        // Assert
        Assert.AreEqual(expected: "::TestTable", actual);
    }

    [TestMethod]
    public void ToString_ReturnsExpectedFormat_WhenTableNameIsNull()
    {
        // Arrange
        var entityInfo = new TableEntityInfo
        {
            ClrType = typeof(string),
            TableName = null!
        };

        // Act
        var actual = entityInfo.ToString();

        // Assert
        Assert.AreEqual(expected: "System.String::", actual);
    }

    [TestMethod]
    public void ToString_ReturnsExpectedFormat_WhenBothClrTypeAndTableNameAreNull()
    {
        // Arrange
        var entityInfo = new TableEntityInfo
        {
            ClrType = null!,
            TableName = null!
        };

        // Act
        var actual = entityInfo.ToString();

        // Assert
        Assert.AreEqual(expected: "::", actual);
    }
}