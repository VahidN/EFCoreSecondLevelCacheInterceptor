using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class StringExtensionsTests
{
    [Fact]
    public void EndsWith_ReturnsFalse_WhenValueIsNull()
    {
        // Arrange
        var collection = new List<string>
        {
            "testValue",
            "anotherValue"
        };

        // Act
        var actual = collection.EndsWith(value: null, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void EndsWith_ReturnsTrue_WhenCollectionContainsItemEndingWithValue()
    {
        // Arrange
        var collection = new List<string>
        {
            "testValue",
            "anotherValue"
        };

        // Act
        var actual = collection.EndsWith(value: "Value", StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void EndsWith_ReturnsFalse_WhenCollectionDoesNotContainItemEndingWithValue()
    {
        // Arrange
        var collection = new List<string>
        {
            "testValue",
            "anotherValue"
        };

        // Act
        var actual = collection.EndsWith(value: "NotExist", StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void StartsWith_ReturnsFalse_WhenValueIsNull()
    {
        // Arrange
        var collection = new List<string>
        {
            "ValueTest",
            "ValueAnother"
        };

        // Act
        var actual = collection.StartsWith(value: null, StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void StartsWith_ReturnsTrue_WhenCollectionContainsItemStartingWithValue()
    {
        // Arrange
        var collection = new List<string>
        {
            "ValueTest",
            "ValueAnother"
        };

        // Act
        var actual = collection.StartsWith(value: "Value", StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void StartsWith_ReturnsFalse_WhenCollectionDoesNotContainItemStartingWithValue()
    {
        // Arrange
        var collection = new List<string>
        {
            "ValueTest",
            "ValueAnother"
        };

        // Act
        var actual = collection.StartsWith(value: "NotExist", StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ContainsEvery_ReturnsFalse_WhenCollectionIsNull()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        // Act
        var actual = source.ContainsEvery(collection: null, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ContainsEvery_ReturnsTrue_WhenSourceContainsEveryItemInCollection()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        var collection = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        // Act
        var actual = source.ContainsEvery(collection, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ContainsEvery_ReturnsFalse_WhenSourceDoesNotContainEveryItemInCollection()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item2"
        };

        var collection = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        // Act
        var actual = source.ContainsEvery(collection, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ContainsOnly_ReturnsFalse_WhenCollectionIsNull()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item2"
        };

        // Act
        var actual = source.ContainsOnly(collection: null, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void ContainsOnly_ReturnsTrue_WhenSourceContainsOnlyItemsInCollection()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item2"
        };

        var collection = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        // Act
        var actual = source.ContainsOnly(collection, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void ContainsOnly_ReturnsFalse_WhenSourceContainsItemsNotInCollection()
    {
        // Arrange
        var source = new List<string>
        {
            "item1",
            "item4"
        };

        var collection = new List<string>
        {
            "item1",
            "item2",
            "item3"
        };

        // Act
        var actual = source.ContainsOnly(collection, StringComparer.OrdinalIgnoreCase);

        // Assert
        Assert.False(actual);
    }
}