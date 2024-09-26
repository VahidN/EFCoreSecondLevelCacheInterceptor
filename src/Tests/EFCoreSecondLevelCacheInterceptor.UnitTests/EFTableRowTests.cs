namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFTableRowTests
{
    [Fact]
    public void Constructor_ShouldInitializeValues()
    {
        // Arrange
        var values = new List<object> { 1, "test" };

        // Act
        var row = new EFTableRow(values);

        // Assert
        Assert.Equal(values, row.Values);
    }

    [Fact]
    public void GetValues_ShouldReturnExpectedValues()
    {
        // Arrange
        var expected = new List<object> { 1, "test" };
        var row = new EFTableRow(expected);

        // Act
        var actual = row.Values;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetDepth_ShouldReturnExpectedCount()
    {
        // Arrange
        const int expected = 1;
        var values = new List<object> { 1, "test" };
        var row = new EFTableRow(values) { Depth = expected };

        // Act
        var actual = row.Depth;

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetFieldCount_ShouldReturnExpectedCount()
    {
        // Arrange
        var values = new List<object> { 1, "test" };
        var row = new EFTableRow(values);

        // Act
        var fieldCount = row.FieldCount;

        // Assert
        Assert.Equal(values.Count, fieldCount);
    }

    [Fact]
    public void GetByIndexer_ShouldReturnExpectedValue()
    {
        // Arrange
        var expected = Guid.NewGuid().ToString();
        var values = new List<object> { 1, expected };
        var row = new EFTableRow(values);

        // Act
        var value = row[1];

        // Assert
        Assert.Equal(expected, value);
    }
}