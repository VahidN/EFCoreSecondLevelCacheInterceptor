using Microsoft.Extensions.Options;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFCacheServiceCheckTests
{
    private readonly Mock<IEFCacheServiceProvider> _cacheServiceProviderMock;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly IEFCacheServiceCheck _serviceCheck;

    public EFCacheServiceCheckTests()
    {
        var cacheOptionsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        _cacheServiceProviderMock = new Mock<IEFCacheServiceProvider>();
        _cacheSettings = new EFCoreSecondLevelCacheSettings();

        cacheOptionsMock.Setup(c => c.Value).Returns(_cacheSettings);

        _serviceCheck = new EFCacheServiceCheck(cacheOptionsMock.Object, _cacheServiceProviderMock.Object);
    }

    [Fact]
    public void EFCacheServiceCheck_ThrowsArgumentNullException_WhenCacheSettingsIsNull()
    {
        // Arrange
        var cacheServiceProviderMock = new Mock<IEFCacheServiceProvider>();

        // Act && Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new EFCacheServiceCheck(null, cacheServiceProviderMock.Object));
    }

    [Fact]
    public void EFCacheServiceCheck_CreatesInstanceSuccessfully()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings());

        var cacheServiceProviderMock = new Mock<IEFCacheServiceProvider>();

        // Act
        var serviceCheck = new EFCacheServiceCheck(cacheSettingsMock.Object, cacheServiceProviderMock.Object);

        // Assert
        Assert.NotNull(serviceCheck);
    }

    [Fact]
    public void EFCacheServiceCheck_ShouldNotThrowArgumentNullException_WhenCacheServiceProviderIsNull()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings());

        // ReSharper disable once AssignNullToNotNullAttribute
        // ReSharper disable once ObjectCreationAsStatement
        void Act() => new EFCacheServiceCheck(cacheSettingsMock.Object, null);

        // Act
        var actual = Record.Exception(Act);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void IsCacheServiceAvailable_ReturnsFalse_WhenIsCachingInterceptorEnabledIsFalse()
    {
        // Arrange
        _cacheSettings.IsCachingInterceptorEnabled = false;

        // Act
        var result = _serviceCheck.IsCacheServiceAvailable();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsCacheServiceAvailable_ReturnsTrue_WhenUseDbCallsIfCachingProviderIsDownIsFalse()
    {
        // Arrange
        _cacheSettings.IsCachingInterceptorEnabled = true;
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = false;

        // Act
        var result = _serviceCheck.IsCacheServiceAvailable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void
        IsCacheServiceAvailable_ReturnsTrue_WhenUseDbCallsIfCachingProviderIsDownIsTrue_And_CacheServerIsAvailable()
    {
        // Arrange
        _cacheSettings.IsCachingInterceptorEnabled = true;
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = true;

        _cacheServiceProviderMock
            .Setup(c => c.GetValue(It.IsAny<EFCacheKey>(), It.IsAny<EFCachePolicy>()))
            .Returns(new EFCachedData());

        // Act
        var result = _serviceCheck.IsCacheServiceAvailable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void
        IsCacheServiceAvailable_ThrowsInvalidOperationException_WhenUseDbCallsIfCachingProviderIsDownIsTrue_And_CacheServerIsNotAvailable()
    {
        // Arrange
        _cacheSettings.IsCachingInterceptorEnabled = true;
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = true;

        _cacheServiceProviderMock
            .Setup(c => c.GetValue(It.IsAny<EFCacheKey>(), It.IsAny<EFCachePolicy>()))
            .Throws<InvalidOperationException>();

        // Act
        void Act() => _serviceCheck.IsCacheServiceAvailable();

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void IsCacheServiceAvailable_ReturnsTrue_WhenNotEnoughTimeHasPassedSinceTheLastCheck()
    {
        // Arrange
        _cacheSettings.IsCachingInterceptorEnabled = true;
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = true;
        _cacheSettings.NextCacheServerAvailabilityCheck = TimeSpan.MaxValue;

        _cacheServiceProviderMock
            .Setup(c => c.GetValue(It.IsAny<EFCacheKey>(), It.IsAny<EFCachePolicy>()))
            .Returns(new EFCachedData());

        _serviceCheck.IsCacheServiceAvailable();

        // Act
        var result = _serviceCheck.IsCacheServiceAvailable();

        // Assert
        Assert.True(result);
    }
}