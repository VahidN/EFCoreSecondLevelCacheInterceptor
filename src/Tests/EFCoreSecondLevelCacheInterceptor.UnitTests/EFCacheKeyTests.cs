using Microsoft.EntityFrameworkCore;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFCacheKeyTests
{
    [Fact]
    public void Equals_ReturnsTrue_WhenKeyHashAndDbContextAreEqual()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var cacheKey2 = new EFCacheKey(new HashSet<string>
        {
            "Entity2"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        // Act
        var actual = cacheKey1.Equals(cacheKey2);

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenObjIsNotEFCacheKey()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var obj = new object();

        // Act
        var actual = cacheKey1.Equals(obj);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenKeyHashIsDifferent()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var cacheKey2 = new EFCacheKey(new HashSet<string>
        {
            "Entity2"
        })
        {
            KeyHash = "hash2",
            DbContext = typeof(DbContext)
        };

        // Act
        var actual = cacheKey1.Equals(cacheKey2);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDbContextIsDifferent()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var cacheKey2 = new EFCacheKey(new HashSet<string>
        {
            "Entity2"
        })
        {
            KeyHash = "hash1",
            DbContext = Mock.Of<DbContext>().GetType()
        };

        // Act
        var actual = cacheKey1.Equals(cacheKey2);

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public void GetHashCode_ReturnsSameHashCode_ForEqualObjects()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var cacheKey2 = new EFCacheKey(new HashSet<string>
        {
            "Entity2"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        // Act
        var hashCode1 = cacheKey1.GetHashCode();
        var hashCode2 = cacheKey2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ReturnsDifferentHashCode_ForDifferentObjects()
    {
        // Arrange
        var cacheKey1 = new EFCacheKey(new HashSet<string>
        {
            "Entity1"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        var cacheKey2 = new EFCacheKey(new HashSet<string>
        {
            "Entity2"
        })
        {
            KeyHash = "hash2",
            DbContext = typeof(DbContext)
        };

        // Act
        var hashCode1 = cacheKey1.GetHashCode();
        var hashCode2 = cacheKey2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var cacheKey = new EFCacheKey(new HashSet<string>
        {
            "Entity1",
            "Entity2"
        })
        {
            KeyHash = "hash1",
            DbContext = typeof(DbContext)
        };

        // Act
        var actual = cacheKey.ToString();

        // Assert
        Assert.Equal(expected: "KeyHash: hash1, DbContext: DbContext, CacheDependencies: Entity1, Entity2.", actual);
    }
}