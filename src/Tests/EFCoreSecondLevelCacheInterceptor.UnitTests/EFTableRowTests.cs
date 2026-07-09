namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]

// ReSharper disable once InconsistentNaming
public class EFTableRowTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeValues()
    {
        // Arrange
        var values = new List<object>
        {
            1,
            "test"
        };

        // Act
        var row = new EFTableRow(values);

        // Assert
        Assert.AreEqual(values, row.Values);
    }

    [TestMethod]
    public void GetValues_ShouldReturnExpectedValues()
    {
        // Arrange
        var expected = new List<object>
        {
            1,
            "test"
        };

        var row = new EFTableRow(expected);

        // Act
        var actual = row.Values;

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetDepth_ShouldReturnExpectedCount()
    {
        // Arrange
        const int expected = 1;

        var values = new List<object>
        {
            1,
            "test"
        };

        var row = new EFTableRow(values)
        {
            Depth = expected
        };

        // Act
        var actual = row.Depth;

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetFieldCount_ShouldReturnExpectedCount()
    {
        // Arrange
        var values = new List<object>
        {
            1,
            "test"
        };

        var row = new EFTableRow(values);

        // Act
        var fieldCount = row.FieldCount;

        // Assert
        Assert.AreEqual(values.Count, fieldCount);
    }

    [TestMethod]
    public void GetByIndexer_ShouldReturnExpectedValue()
    {
        // Arrange
        var expected = Guid.NewGuid().ToString();

        var values = new List<object>
        {
            1,
            expected
        };

        var row = new EFTableRow(values);

        // Act
        var value = row[ordinal: 1];

        // Assert
        Assert.AreEqual(expected, value);
    }
}