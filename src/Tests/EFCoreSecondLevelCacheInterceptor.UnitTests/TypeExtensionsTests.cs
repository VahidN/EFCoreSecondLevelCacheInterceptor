using System.Collections;


namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]
public class TypeExtensionsTests
{
    [TestMethod]
    public void IsNull_ShouldReturnTrue_WhenValueIsNull()
    {
        // Arrange && Act
        var actual = ((object)null).IsNull();

        // Assert
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public void IsNull_ShouldReturnTrue_WhenValueIsDBNull()
    {
        // Arrange
        object value = DBNull.Value;

        // Act
        var actual = value.IsNull();

        // Assert
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public void IsNull_ShouldReturnFalse_WhenValueIsNotNullOrDBNull()
    {
        // Arrange
        var value = new object();

        // Act
        var actual = value.IsNull();

        // Assert
        Assert.IsTrue(!actual);
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow(typeof(int), false)]
    [DataRow(typeof(int[]), true)]
    [DataRow(typeof(ArrayList), false)]
    [DataRow(typeof(LinkedList<>), false)]
    [DataRow(typeof(LinkedList<int>), false)]
    [DataRow(typeof(List<>), true)]
    [DataRow(typeof(List<int>), true)]
    [DataRow(typeof(SortedList<,>), false)]
    [DataRow(typeof(SortedList<int, int>), false)]
    public void IsArrayOrGenericList_ShouldReturnExpectedResult(Type type, bool expected)
    {
        // Arrange && Act
        var actual = TypeExtensions.IsArrayOrGenericList(type);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(null, false)]
    [DataRow(typeof(bool), false)]
    [DataRow(typeof(byte), true)]
    [DataRow(typeof(sbyte), true)]
    [DataRow(typeof(char), true)]
    [DataRow(typeof(decimal), true)]
    [DataRow(typeof(double), true)]
    [DataRow(typeof(float), true)]
    [DataRow(typeof(int), true)]
    [DataRow(typeof(uint), true)]
    [DataRow(typeof(nint), false)]
    [DataRow(typeof(nuint), false)]
    [DataRow(typeof(long), true)]
    [DataRow(typeof(ulong), true)]
    [DataRow(typeof(short), true)]
    [DataRow(typeof(ushort), true)]
    [DataRow(typeof(DateOnly), false)]
    [DataRow(typeof(DateTime), false)]
    [DataRow(typeof(DateTimeOffset), false)]
    [DataRow(typeof(TimeOnly), false)]
    [DataRow(typeof(TimeSpan), false)]
    [DataRow(typeof(Guid), false)]
    [DataRow(typeof(object), false)]
    [DataRow(typeof(string), false)]
    public void IsNumber_ShouldReturnExpectedResult(Type type, bool expected)
    {
        // Arrange && Act
        var actual = TypeExtensions.IsNumber(type);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}