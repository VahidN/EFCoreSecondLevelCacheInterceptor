using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

public class DbCommandInterceptorProcessorTests
{
    private readonly IDbCommandInterceptorProcessor _processor;
    private readonly Mock<IEFDebugLogger> _loggerMock;
    private readonly Mock<ILogger<DbCommandInterceptorProcessor>> _interceptorProcessorLoggerMock;
    private readonly Mock<IEFCacheServiceProvider> _cacheServiceMock;
    private readonly Mock<IEFCacheDependenciesProcessor> _cacheDependenciesProcessorMock;
    private readonly Mock<IEFCacheKeyProvider> _cacheKeyProviderMock;
    private readonly Mock<IEFCachePolicyParser> _cachePolicyParserMock;
    private readonly Mock<IEFSqlCommandsProcessor> _sqlCommandsProcessorMock;
    private readonly Mock<IOptions<EFCoreSecondLevelCacheSettings>> _cacheSettingsMock;
    private readonly Mock<IEFCacheServiceCheck> _cacheServiceCheckMock;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

    public DbCommandInterceptorProcessorTests()
    {
        _loggerMock = new Mock<IEFDebugLogger>();
        _interceptorProcessorLoggerMock = new Mock<ILogger<DbCommandInterceptorProcessor>>();
        _cacheServiceMock = new Mock<IEFCacheServiceProvider>();
        _cacheDependenciesProcessorMock = new Mock<IEFCacheDependenciesProcessor>();
        _cacheKeyProviderMock = new Mock<IEFCacheKeyProvider>();
        _cachePolicyParserMock = new Mock<IEFCachePolicyParser>();
        _sqlCommandsProcessorMock = new Mock<IEFSqlCommandsProcessor>();
        _cacheSettingsMock = new Mock<IOptions<EFCoreSecondLevelCacheSettings>>();
        _cacheServiceCheckMock = new Mock<IEFCacheServiceCheck>();
        _cacheSettings = new EFCoreSecondLevelCacheSettings();

        _cacheSettingsMock.SetupGet(x => x.Value).Returns(_cacheSettings);

        _processor = new DbCommandInterceptorProcessor(
            _loggerMock.Object,
            _interceptorProcessorLoggerMock.Object,
            _cacheServiceMock.Object,
            _cacheDependenciesProcessorMock.Object,
            _cacheKeyProviderMock.Object,
            _cachePolicyParserMock.Object,
            _sqlCommandsProcessorMock.Object,
            _cacheSettingsMock.Object,
            _cacheServiceCheckMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCacheSettingsIsNull()
    {
        // Arrange
        var logger = new Mock<IEFDebugLogger>().Object;
        var interceptorProcessorLogger = new Mock<ILogger<DbCommandInterceptorProcessor>>().Object;
        var cacheService = new Mock<IEFCacheServiceProvider>().Object;
        var cacheDependenciesProcessor = new Mock<IEFCacheDependenciesProcessor>().Object;
        var cacheKeyProvider = new Mock<IEFCacheKeyProvider>().Object;
        var cachePolicyParser = new Mock<IEFCachePolicyParser>().Object;
        var sqlCommandsProcessor = new Mock<IEFSqlCommandsProcessor>().Object;
        var cacheServiceCheck = new Mock<IEFCacheServiceCheck>().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptorProcessor(
            logger,
            interceptorProcessorLogger,
            cacheService,
            cacheDependenciesProcessor,
            cacheKeyProvider,
            cachePolicyParser,
            sqlCommandsProcessor,
            null,
            cacheServiceCheck));
    }

    [Fact]
    public void Constructor_InitializesNewInstance()
    {
        // Arrange
        var logger = new Mock<IEFDebugLogger>().Object;
        var interceptorProcessorLogger = new Mock<ILogger<DbCommandInterceptorProcessor>>().Object;
        var cacheService = new Mock<IEFCacheServiceProvider>().Object;
        var cacheDependenciesProcessor = new Mock<IEFCacheDependenciesProcessor>().Object;
        var cacheKeyProvider = new Mock<IEFCacheKeyProvider>().Object;
        var cachePolicyParser = new Mock<IEFCachePolicyParser>().Object;
        var sqlCommandsProcessor = new Mock<IEFSqlCommandsProcessor>().Object;
        var cacheSettings = Options.Create(new EFCoreSecondLevelCacheSettings());
        var cacheServiceCheck = new Mock<IEFCacheServiceCheck>().Object;

        // Act
        var processor = new DbCommandInterceptorProcessor(
            logger,
            interceptorProcessorLogger,
            cacheService,
            cacheDependenciesProcessor,
            cacheKeyProvider,
            cachePolicyParser,
            sqlCommandsProcessor,
            cacheSettings,
            cacheServiceCheck);

        // Assert
        Assert.NotNull(processor);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenDbContextIsNull()
    {
        // Arrange
        DbContext context = null;

        // Act
        var actual = _processor.ProcessExecutedCommands<object>(null, context, null);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenCommandIsNull()
    {
        // Arrange
        var context = Mock.Of<DbContext>();

        // Act
        var actual = _processor.ProcessExecutedCommands<object>(null, context, null);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ThrowsInvalidOperationException_WhenNotUseDbCallsIfCachingProviderIsDown()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Throws<InvalidOperationException>();

        // Act
        void Act() => _processor.ProcessExecutedCommands(command, context, expected);

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCachingErrorEvent_WhenThrowsInvalidOperationException()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Throws<InvalidOperationException>();
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = true;

        // Act
        _processor.ProcessExecutedCommands(command, context, expected);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingError,
            It.Is<string>(message =>
                message.Contains(
                    "System.InvalidOperationException: Operation is not valid due to the current state of the object.")),
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenCacheServiceIsNotAvailable()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(false);

        // Act
        var actual = _processor.ProcessExecutedCommands(command, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void
        ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenSkipCachingDbContextsSettingIsNotNullAndContainsType()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.SkipCachingDbContexts = new List<Type> { context.GetType() };

        // Act
        var actual = _processor.ProcessExecutedCommands(command, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void
        ProcessExecutedCommands_NotifiesCachingSkippedEvent_WhenSkipCachingDbContextsSettingIsNotNullAndContainsTypeAndLoggerEnabled()
    {
        // Arrange
        var result = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.SkipCachingDbContexts = new List<Type> { context.GetType() };

        // Act
        var actual = _processor.ProcessExecutedCommands(command, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped caching of this DbContext: Castle.Proxies.DbContextProxy",
            string.Empty), Times.Once);
    }

    [Fact]
    public void
        ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenShouldSkipQueriesInsideExplicitTransactionAndTransactionIsNotNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenCachePolicyIsNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenIsCrudCommandAndCachePolicyIsNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void
        ProcessExecutedCommands_ReturnsExpectedResultWithoutPrecessing_WhenInvalidateCacheDependenciesReturnsTrue()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheDependenciesProcessorMock.Setup(x => x.InvalidateCacheDependencies(null, efCacheKey)).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCachingSkippedEvent_WhenIsCrudCommandAndCachePolicyIsNull()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipping a none-cachable command[].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsCachedTableRows()
    {
        // Arrange
        var expected = new EFTableRowsDataReader(new EFTableRows());
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCacheHitEvent_WhenReturningCachedTableRows()
    {
        // Arrange
        var result = new EFTableRowsDataReader(new EFTableRows
        {
            TableName = string.Empty
        });

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CacheHit,
            "Returning the cached TableRows[].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_SkipsCachingResultsIfResultIsIntType()
    {
        // Arrange
        const int expected = int.MaxValue;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.NonQuery == expected),
            cachePolicy), Times.Never);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCacheHitEvent_WhenSkipsCachingResultsIfResultIsIntType()
    {
        // Arrange
        const int result = int.MaxValue;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped caching of this result based on the provided predicate.",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_AddsIntDataToCache()
    {
        // Arrange
        const int expected = int.MaxValue;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.NonQuery == expected),
            cachePolicy), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesQueryResultCachedEvent_WhenIntDataAddedToCache()
    {
        // Arrange
        const int result = int.MaxValue;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultCached,
            "[2147483647] added to the cache[KeyHash: , DbContext: , CacheDependencies: .].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResult_WhenIntDataAddedToCache()
    {
        // Arrange
        const int expected = int.MaxValue;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsEFTableRowsDataReader_WhenSkipsCachingResultsIfResultIsDbDataReaderType()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var dataReaderMock = new Mock<DbDataReader>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, dataReaderMock.Object);

        // Assert
        Assert.IsType<EFTableRowsDataReader>(actual);
    }

    [Fact]
    public void ProcessExecutedCommands_SkipsCachingResultsIfResultIsDbDataReaderType()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var dataReaderMock = new Mock<DbDataReader>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, dataReaderMock.Object);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.TableRows != null),
            cachePolicy), Times.Never);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCacheHitEvent_WhenSkipsCachingResultsIfDataIsEFTableRowsType()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var dataReaderMock = new Mock<DbDataReader>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, dataReaderMock.Object);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped caching of this result based on the provided predicate.",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_AddsEFTableRowsDataToCache()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var dataReaderMock = new Mock<DbDataReader>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, dataReaderMock.Object);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.TableRows != null),
            cachePolicy), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesQueryResultCachedEvent_WhenEFTableRowsDataAddedToCache()
    {
        // Arrange
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var dataReaderMock = new Mock<DbDataReader>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, dataReaderMock.Object);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultCached,
            It.Is<string>(message =>
                message.Contains(" added to the cache[KeyHash: , DbContext: , CacheDependencies: .].")),
            null), Times.Once);
    }


    [Fact]
    public void ProcessExecutedCommands_SkipsCachingResultsIfResultIsObjectType()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.Scalar == result),
            cachePolicy), Times.Never);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesCacheHitEvent_WhenSkipsCachingResultsIfResultIsObjectType()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;
        _cacheSettings.SkipCachingResults = _ => true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped caching of this result based on the provided predicate.",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_AddsObjectDataToCache()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _cacheServiceMock.Verify(x => x.InsertValue(
            efCacheKey,
            It.Is<EFCachedData>(data => data.Scalar == result),
            cachePolicy), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_NotifiesQueryResultCachedEvent_WhenObjectDataAddedToCache()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutedCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultCached,
            "[System.Object] added to the cache[KeyHash: , DbContext: , CacheDependencies: .].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsExpectedResult_WhenObjectDataAddedToCache()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutedCommands_ReturnsNull_WhenResultIsNull()
    {
        // Arrange
        object expected = null;

        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutedCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenDbContextIsNull()
    {
        // Arrange
        DbContext context = null;

        // Act
        var actual = _processor.ProcessExecutingCommands<object>(null, context, null);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenCommandIsNull()
    {
        // Arrange
        var context = Mock.Of<DbContext>();

        // Act
        var actual = _processor.ProcessExecutingCommands<object>(null, context, null);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ProcessExecutingCommands_ThrowsInvalidOperationException_WhenNotUseDbCallsIfCachingProviderIsDown()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Throws<InvalidOperationException>();

        // Act
        void Act() => _processor.ProcessExecutingCommands(command, context, expected);

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesCachingErrorEvent_WhenThrowsInvalidOperationException()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Throws<InvalidOperationException>();
        _cacheSettings.UseDbCallsIfCachingProviderIsDown = true;

        // Act
        _processor.ProcessExecutingCommands(command, context, expected);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingError,
            It.Is<string>(message =>
                message.Contains(
                    "System.InvalidOperationException: Operation is not valid due to the current state of the object.")),
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenCacheServiceIsNotAvailable()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(false);

        // Act
        var actual = _processor.ProcessExecutingCommands(command, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void
        ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenSkipCachingDbContextsSettingIsNotNullAndContainsType()
    {
        // Arrange
        var expected = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.SkipCachingDbContexts = new List<Type> { context.GetType() };

        // Act
        var actual = _processor.ProcessExecutingCommands(command, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void
        ProcessExecutingCommands_NotifiesCachingSkippedEvent_WhenSkipCachingDbContextsSettingIsNotNullAndContainsTypeAndLoggerEnabled()
    {
        // Arrange
        var result = new object();
        var command = Mock.Of<DbCommand>();
        var context = Mock.Of<DbContext>();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.SkipCachingDbContexts = new List<Type> { context.GetType() };

        // Act
        _processor.ProcessExecutingCommands(command, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped caching of this DbContext: Castle.Proxies.DbContextProxy",
            string.Empty), Times.Once);
    }

    [Fact]
    public void
        ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenShouldSkipQueriesInsideExplicitTransactionAndTransactionIsNotNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenCachePolicyIsNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenIsCrudCommandAndCachePolicyIsNull()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesCachingSkippedEvent_WhenCachePolicyIsNull()
    {
        // Arrange
        var result = new object();
        var commandMock = new Mock<DbCommand>();
        var transaction = Mock.Of<DbTransaction>();
        var context = Mock.Of<DbContext>();

        commandMock.Protected().Setup<DbTransaction>("DbTransaction").Returns(transaction);

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipping a none-cachable command[].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsExpectedResultWithoutPrecessing_WhenCacheKeyWasNotPresentInTheCache()
    {
        // Arrange
        var expected = new object();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesQueryResultSuppressedEvent_WhenThrowsInvalidOperationException()
    {
        // Arrange
        var result = new InterceptionResult<DbDataReader>();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultSuppressed,
            "Suppressed the result with an empty TableRows.",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsSuppressedResult()
    {
        // Arrange
        var result = new InterceptionResult<DbDataReader>();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        Assert.IsType<InterceptionResult<DbDataReader>>(actual);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesQueryResultSuppressedEvent_WhenCacheResultIsNutNull()
    {
        // Arrange
        var result = new InterceptionResult<DbDataReader>();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData
        {
            IsNull = false,
            TableRows = new EFTableRows
            {
                TableName = string.Empty
            }
        };

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultSuppressed,
            "Suppressed the result with the TableRows[] from the cache[KeyHash: , DbContext: , CacheDependencies: .].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesQueryResultSuppressedEvent_WhenInterceptionResultGenericIsIntType()
    {
        // Arrange
        var result = new InterceptionResult<int>();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData
        {
            IsNull = false,
            NonQuery = int.MaxValue
        };

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultSuppressed,
            "Suppressed the result with 2147483647 from the cache[KeyHash: , DbContext: , CacheDependencies: .].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsCachedResult_WhenInterceptionResultGenericIsIntType()
    {
        // Arrange
        const int expected = int.MaxValue;

        var result = InterceptionResult<int>.SuppressWithResult(expected);
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData
        {
            IsNull = false,
            NonQuery = expected
        };

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        Assert.Equal(expected, actual.Result);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesQueryResultSuppressedEvent_WhenInterceptionResultGenericIsObjectType()
    {
        // Arrange
        var expected = new object();
        var result = new InterceptionResult<object>();
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData
        {
            IsNull = false,
            Scalar = expected
        };

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.QueryResultSuppressed,
            "Suppressed the result with System.Object from the cache[KeyHash: , DbContext: , CacheDependencies: .].",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsCachedResult_WhenInterceptionResultGenericIsObjectType()
    {
        // Arrange
        var expected = new object();
        var result = InterceptionResult<object>.SuppressWithResult(expected);
        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData
        {
            IsNull = false,
            Scalar = expected
        };

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        Assert.Equal(expected, actual.Result);
    }

    [Fact]
    public void ProcessExecutingCommands_NotifiesCachingSkippedEvent_WhenResultIsNull()
    {
        // Arrange
        object result = null;

        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        _loggerMock.Verify(x => x.NotifyCacheableEvent(
            CacheableLogEventId.CachingSkipped,
            "Skipped the result with  type.",
            null), Times.Once);
    }

    [Fact]
    public void ProcessExecutingCommands_ReturnsNull_WhenResultIsNull()
    {
        // Arrange
        object result = null;

        var commandMock = new Mock<DbCommand>();
        var context = Mock.Of<DbContext>();
        var cachePolicy = new EFCachePolicy();
        var efCacheKey = new EFCacheKey(new HashSet<string>());
        var cacheResult = new EFCachedData();

        _loggerMock.SetupGet(x => x.IsLoggerEnabled).Returns(true);
        _cacheServiceMock.Setup(x => x.GetValue(efCacheKey, cachePolicy)).Returns(cacheResult);
        _sqlCommandsProcessorMock.Setup(x => x.IsCrudCommand(string.Empty)).Returns(true);
        _cacheServiceCheckMock.Setup(x => x.IsCacheServiceAvailable()).Returns(true);
        _cacheKeyProviderMock.Setup(x => x.GetEFCacheKey(commandMock.Object, context, cachePolicy)).Returns(efCacheKey);
        _cachePolicyParserMock.Setup(x => x.GetEFCachePolicy(string.Empty, null)).Returns(cachePolicy);
        _cacheSettings.AllowCachingWithExplicitTransactions = true;

        // Act
        var actual = _processor.ProcessExecutingCommands(commandMock.Object, context, result);

        // Assert
        Assert.Null(actual);
    }
}