using Microsoft.Extensions.Options;
using Moq;
using Assert = Xunit.Assert;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFCacheKeyPrefixProviderTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCacheSettingsIsNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>().Object;

        // ReSharper disable once ObjectCreationAsStatement
        void Act() => new EFCacheKeyPrefixProvider(serviceProvider, null!);

        // Act && Assert
        Assert.Throws<ArgumentNullException>(paramName: "cacheSettings", Act);
    }

    [Fact]
    public void GetCacheKeyPrefix_ReturnsCacheKeyPrefixSelectorResult_WhenSelectorIsNotNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>().Object;

        var cacheSettings = Options.Create(new EFCoreSecondLevelCacheSettings
        {
            CacheKeyPrefixSelector = _ => "CustomPrefix"
        });

        var provider = new EFCacheKeyPrefixProvider(serviceProvider, cacheSettings);

        // Act
        var actual = provider.GetCacheKeyPrefix();

        // Assert
        Assert.Equal(expected: "CustomPrefix", actual);
    }

    [Fact]
    public void GetCacheKeyPrefix_ReturnsCacheKeyPrefix_WhenSelectorIsNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>().Object;

        var cacheSettings = Options.Create(new EFCoreSecondLevelCacheSettings
        {
            CacheKeyPrefix = "DefaultPrefix",
            CacheKeyPrefixSelector = null
        });

        var provider = new EFCacheKeyPrefixProvider(serviceProvider, cacheSettings);

        // Act
        var actual = provider.GetCacheKeyPrefix();

        // Assert
        Assert.Equal(expected: "DefaultPrefix", actual);
    }
}