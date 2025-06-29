using Microsoft.Extensions.Options;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class EFMessagePackSerializerTests
{
    private static Mock<IOptions<EFCoreSecondLevelCacheSettings>> GetCacheSettingsMock(bool enableCompression)
    {
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        cacheSettingsMock.Setup(c => c.Value)
            .Returns(new EFCoreSecondLevelCacheSettings
            {
                AdditionalData = new EFRedisCacheConfigurationOptions
                {
                    EnableCompression = enableCompression
                }
            });

        return cacheSettingsMock;
    }

    [Fact]
    public void SerializeAndDeserializeWhenObjectIsComplexAndCompressionDisabledShouldReturnEqualObject()
    {
        // Arrange
        var serializer = new EFMessagePackSerializer(GetCacheSettingsMock(enableCompression: false).Object);

        var originalObject = new Person
        {
            Id = 1,
            Name = "John Doe",
            BirthDate = new DateTimeOffset(year: 1990, month: 5, day: 20, hour: 10, minute: 30, second: 0,
                TimeSpan.FromHours(value: 3.5)),
            Salary = 60000.50m,
            UniqueId = Guid.NewGuid()
        };

        // Act
        var serializedData = serializer.Serialize(originalObject);
        var deserializedObject = serializer.Deserialize<Person>(serializedData);

        // Assert
        Assert.NotNull(serializedData);
        Assert.True(serializedData.Length > 0);
        Assert.NotNull(deserializedObject);
        Assert.Equal(originalObject, deserializedObject);
    }

    [Fact]
    public void SerializeAndDeserializeWhenObjectIsComplexAndCompressionEnabledShouldReturnEqualObject()
    {
        // Arrange
        var serializer = new EFMessagePackSerializer(GetCacheSettingsMock(enableCompression: true).Object);

        var originalObject = new Person
        {
            Id = 100,
            Name = "Jane Smith",
            BirthDate = new DateTimeOffset(year: 1990, month: 5, day: 20, hour: 10, minute: 30, second: 0,
                TimeSpan.FromHours(value: 3.5)),
            Salary = 120000.75m,
            UniqueId = Guid.NewGuid()
        };

        // Act
        var serializedData = serializer.Serialize(originalObject);
        var deserializedObject = serializer.Deserialize<Person>(serializedData);

        // Assert
        Assert.NotNull(serializedData);
        Assert.True(serializedData.Length > 0);
        Assert.NotNull(deserializedObject);
        Assert.Equal(originalObject, deserializedObject);
    }

    [Fact]
    public void SerializeWhenObjectIsNullShouldReturnValidByteArrayForNull()
    {
        // Arrange
        var serializer = new EFMessagePackSerializer(GetCacheSettingsMock(enableCompression: false).Object);

        // Act
        var serializedData = serializer.Serialize<object>(obj: null);
        var deserializedObject = serializer.Deserialize<object>(serializedData);

        // Assert
        // MessagePack 'nil' is represented by a single byte 0xc0
        Assert.NotNull(serializedData);
        Assert.Single(serializedData);
        Assert.Equal(expected: 0xc0, serializedData[0]);
        Assert.Null(deserializedObject);
    }

    [Fact]
    public void DeserializeWhenDataIsNullShouldReturnDefaultOfT()
    {
        // Arrange
        var serializer = new EFMessagePackSerializer(GetCacheSettingsMock(enableCompression: false).Object);

        // Act
        var result = serializer.Deserialize<Person>(data: null);

        // Assert
        Assert.Null(result); // default(Person) is null for reference types
    }
}