using System.Collections;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class TypeExtensionsTests
{
    [Fact]
    public void IsNull_ShouldReturnTrue_WhenValueIsNull()
    {
        // Arrange && Act
        var actual = ((object)null).IsNull();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsNull_ShouldReturnTrue_WhenValueIsDBNull()
    {
        // Arrange
        object value = DBNull.Value;

        // Act
        var actual = value.IsNull();

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void IsNull_ShouldReturnFalse_WhenValueIsNotNullOrDBNull()
    {
        // Arrange
        var value = new object();

        // Act
        var actual = value.IsNull();

        // Assert
        Assert.False(actual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(int[]), true)]
    [InlineData(typeof(ArrayList), false)]
    [InlineData(typeof(LinkedList<>), false)]
    [InlineData(typeof(LinkedList<int>), false)]
    [InlineData(typeof(List<>), true)]
    [InlineData(typeof(List<int>), true)]
    [InlineData(typeof(SortedList<,>), false)]
    [InlineData(typeof(SortedList<int, int>), false)]
    public void IsArrayOrGenericList_ShouldReturnExpectedResult(Type type, bool expected)
    {
        // Arrange && Act
        var actual = TypeExtensions.IsArrayOrGenericList(type);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(char), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(nint), false)]
    [InlineData(typeof(nuint), false)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(DateOnly), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(DateTimeOffset), false)]
    [InlineData(typeof(TimeOnly), false)]
    [InlineData(typeof(TimeSpan), false)]
    [InlineData(typeof(Guid), false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(string), false)]
    public void IsNumber_ShouldReturnExpectedResult(Type type, bool expected)
    {
        // Arrange && Act
        var actual = TypeExtensions.IsNumber(type);

        // Assert
        Assert.Equal(expected, actual);
    }
}