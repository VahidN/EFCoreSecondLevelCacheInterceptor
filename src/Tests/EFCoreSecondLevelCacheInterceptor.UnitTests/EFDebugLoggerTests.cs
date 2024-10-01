using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

// ReSharper disable once InconsistentNaming
public class EFDebugLoggerTests
{
    private readonly Mock<Action<EFCacheableLogEvent>> _cacheableEventMock;
    private readonly Mock<Action<EFCacheInvalidationInfo>> _cacheInvalidationEventMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IEFDebugLogger _debugLogger;

    public EFDebugLoggerTests()
    {
        _cacheableEventMock = new Mock<Action<EFCacheableLogEvent>>();
        _cacheInvalidationEventMock = new Mock<Action<EFCacheInvalidationInfo>>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var cacheSettings = new EFCoreSecondLevelCacheSettings
        {
            EnableLogging = true,
            CacheableEvent = _cacheableEventMock.Object,
            CacheInvalidationEvent = _cacheInvalidationEventMock.Object
        };

        cacheSettingsMock.Setup(c => c.Value).Returns(cacheSettings);
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        _debugLogger = new EFDebugLogger(
            cacheSettingsMock.Object,
            loggerMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public void EFDebugLogger_ThrowsArgumentNullException_WhenCacheSettingsIsNull()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        Assert.Throws<ArgumentNullException>(() =>
            new EFDebugLogger(null!, loggerMock.Object, serviceProviderMock.Object));
    }

    [Fact]
    public void EFDebugLogger_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        // Act && Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EFDebugLogger(cacheSettingsMock.Object, null!, serviceProviderMock.Object));
    }

    [Fact]
    public void EFDebugLogger_EnablesLogging_WhenCacheSettingsEnableLoggingIsTrue()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings { EnableLogging = true });
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act
        var logger = new EFDebugLogger(cacheSettingsMock.Object, loggerMock.Object, serviceProviderMock.Object);

        // Assert
        Assert.True(logger.IsLoggerEnabled);
    }

    [Fact]
    public void EFDebugLogger_DisablesLogging_WhenCacheSettingsEnableLoggingIsFalse()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings { EnableLogging = false });

        // Act
        var logger = new EFDebugLogger(cacheSettingsMock.Object, loggerMock.Object, serviceProviderMock.Object);

        // Assert
        Assert.False(logger.IsLoggerEnabled);
    }

    [Fact]
    public void NotifyCacheableEvent_InvokesCacheableEvent_WhenLoggerIsEnabled()
    {
        // Arrange && Act
        _debugLogger.NotifyCacheableEvent(CacheableLogEventId.CachingSystemStarted, "TestMessage", "TestCommand");

        // Assert
        _cacheableEventMock.Verify(e => e.Invoke(It.Is<EFCacheableLogEvent>(
                x =>
                    x.EventId == CacheableLogEventId.CachingSystemStarted
                    && x.Message == "TestMessage"
                    && x.CommandText == "TestCommand"
                    && x.ServiceProvider == _serviceProviderMock.Object)),
            Times.Once);
    }

    [Fact]
    public void NotifyCacheableEvent_DoesNotInvokeCacheableEvent_WhenLoggerIsDisabled()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var cacheableEventMock = new Mock<Action<EFCacheableLogEvent>>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings { EnableLogging = false });
        cacheSettingsMock.Object.Value.CacheableEvent = cacheableEventMock.Object;

        var logger = new EFDebugLogger(cacheSettingsMock.Object, loggerMock.Object, serviceProviderMock.Object);

        // Act
        logger.NotifyCacheableEvent(CacheableLogEventId.CachingSystemStarted, "TestMessage", "TestCommand");

        // Assert
        cacheableEventMock.Verify(e => e.Invoke(It.IsAny<EFCacheableLogEvent>()), Times.Never);
    }

    [Fact]
    public void NotifyCacheableEvent_DoesNotInvokeCacheableEvent_WhenCacheableEventIsNull()
    {
        // Arrange
        var cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<EFDebugLogger>>();
        var cacheableEventMock = new Mock<Action<EFCacheableLogEvent>>();

        cacheSettingsMock.Setup(c => c.Value).Returns(new EFCoreSecondLevelCacheSettings { EnableLogging = true });
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var logger = new EFDebugLogger(cacheSettingsMock.Object, loggerMock.Object, serviceProviderMock.Object);

        // Act
        logger.NotifyCacheableEvent(CacheableLogEventId.CachingSystemStarted, "TestMessage", "TestCommand");

        // Assert
        cacheableEventMock.Verify(e => e.Invoke(It.IsAny<EFCacheableLogEvent>()), Times.Never);
    }

    [Fact]
    public void NotifyCacheInvalidation_InvokesEFCacheInvalidationInfo()
    {
        // Arrange 
        var cacheDependencies = new HashSet<string>();

        // Act
        _debugLogger.NotifyCacheInvalidation(true, cacheDependencies);

        // Assert
        _cacheInvalidationEventMock.Verify(e => e.Invoke(It.Is<EFCacheInvalidationInfo>(
                x =>
                    x.CacheDependencies == cacheDependencies
                    && x.ClearAllCachedEntries == true
                    && x.ServiceProvider == _serviceProviderMock.Object)),
            Times.Once);
    }
}