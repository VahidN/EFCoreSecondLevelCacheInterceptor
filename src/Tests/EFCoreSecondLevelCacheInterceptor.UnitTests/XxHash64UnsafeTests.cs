using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class XxHash64UnsafeTests
{
    [Fact]
    public void ComputeHash_ThrowsArgumentNullException_WhenStringDataIsNull()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => hashProvider.ComputeHash(((string)null)!));
    }

    [Fact]
    public void ComputeHash_ThrowsArgumentNullException_WhenByteArrayDataIsNull()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => hashProvider.ComputeHash(((byte[])null)!));
    }

    [Fact]
    public void ComputeHash_ThrowsArgumentNullException_WhenByteArrayDataWithOffsetIsNull()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() => hashProvider.ComputeHash(null!, offset: 0, len: 0, seed: 0));
    }

    [Fact]
    public void ComputeHash_ReturnsSameHash_ForSameStringData()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();
        var data = "test";

        // Act
        var hash1 = hashProvider.ComputeHash(data);
        var hash2 = hashProvider.ComputeHash(data);

        // Act && Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsSameHash_ForSameByteArrayData()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();
        var data = "test"u8.ToArray();

        // Act
        var hash1 = hashProvider.ComputeHash(data);
        var hash2 = hashProvider.ComputeHash(data);

        // Act && Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsSameHash_ForSameByteArrayDataWithOffset()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();
        var data = "test"u8.ToArray();

        // Act
        var hash1 = hashProvider.ComputeHash(data, offset: 0, data.Length, seed: 0);
        var hash2 = hashProvider.ComputeHash(data, offset: 0, data.Length, seed: 0);

        // Act && Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsDifferentHash_ForDifferentStringData()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();

        // Act
        var hash1 = hashProvider.ComputeHash(data: "test1");
        var hash2 = hashProvider.ComputeHash(data: "test2");

        // Act && Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsDifferentHash_ForDifferentByteArrayData()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();
        var data1 = "test1"u8.ToArray();
        var data2 = "test2"u8.ToArray();

        // Act
        var hash1 = hashProvider.ComputeHash(data1);
        var hash2 = hashProvider.ComputeHash(data2);

        // Act && Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsDifferentHash_ForDifferentByteArrayDataWithOffset()
    {
        // Arrange
        var hashProvider = new XxHash64Unsafe();
        var data1 = "test1"u8.ToArray();
        var data2 = "test2"u8.ToArray();

        // Act
        var hash1 = hashProvider.ComputeHash(data1, offset: 0, data1.Length, seed: 0);
        var hash2 = hashProvider.ComputeHash(data2, offset: 0, data2.Length, seed: 0);

        // Act && Assert
        Assert.NotEqual(hash1, hash2);
    }
}