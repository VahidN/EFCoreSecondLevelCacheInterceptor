namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class TableEntityInfoTests
{
    [Fact]
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
        Assert.Equal("System.String::TestTable", actual);
    }

    [Fact]
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
        Assert.Equal("::TestTable", actual);
    }

    [Fact]
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
        Assert.Equal("System.String::", actual);
    }

    [Fact]
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
        Assert.Equal("::", actual);
    }
}