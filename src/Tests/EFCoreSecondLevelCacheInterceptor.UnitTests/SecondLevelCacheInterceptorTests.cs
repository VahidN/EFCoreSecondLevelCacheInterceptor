using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using AsyncKeyedLock;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[SuppressMessage(category: "ReSharper", checkId: "AssignNullToNotNullAttribute")]
public class SecondLevelCacheInterceptorTests
{
    [Fact]
    public void Constructor_InitializesFields_WhenArgumentsAreValid()
    {
        var processorMock = new Mock<IDbCommandInterceptorProcessor>();
        var lockProviderMock = new Mock<ILockProvider>();

        var interceptor = new SecondLevelCacheInterceptor(processorMock.Object, lockProviderMock.Object);

        Assert.NotNull(interceptor);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenProcessorIsNull()
    {
        var lockProviderMock = new Mock<ILockProvider>();

        Assert.Throws<ArgumentNullException>(()
            => new SecondLevelCacheInterceptor(processor: null, lockProviderMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLockProviderIsNull()
    {
        var processorMock = new Mock<IDbCommandInterceptorProcessor>();

        Assert.Throws<ArgumentNullException>(()
            => new SecondLevelCacheInterceptor(processorMock.Object, lockProvider: null));
    }

    [Fact]
    public void NonQueryExecuted_ShouldLock()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.NonQueryExecuted(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void NonQueryExecuted_ReturnsProcessedResult()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.NonQueryExecuted(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NonQueryExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.NonQueryExecuted(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task NonQueryExecutedAsync_ShouldLock()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.NonQueryExecutedAsync(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task NonQueryExecutedAsync_ReturnsProcessedResult()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.NonQueryExecutedAsync(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task NonQueryExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = 1;
        const int result = 1;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.NonQueryExecutedAsync(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void NonQueryExecuting_ShouldLock()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.NonQueryExecuting(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void NonQueryExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.NonQueryExecuting(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NonQueryExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.NonQueryExecuting(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task NonQueryExecutingAsync_ShouldLock()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.NonQueryExecutingAsync(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task NonQueryExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.NonQueryExecutingAsync(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task NonQueryExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.NonQueryExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ReaderExecuted_ShouldLock()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.ReaderExecuted(command, eventData, expected);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void ReaderExecuted_ReturnsExpectedDbDataReader()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.ReaderExecuted(command, eventData, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ReaderExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ReaderExecuted(command: null, eventData: null, expected));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task ReaderExecutedAsync_ShouldLock()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.ReaderExecutedAsync(command, eventData, expected);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ReaderExecutedAsync_ReturnsExpectedDbDataReader()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.ReaderExecutedAsync(command, eventData, expected);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ReaderExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.ReaderExecutedAsync(command: null, eventData: null, expected));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ReaderExecuting_ShouldLock()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.ReaderExecuting(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void ReaderExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.ReaderExecuting(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ReaderExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ReaderExecuting(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task ReaderExecutingAsync_ShouldLock()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.ReaderExecutingAsync(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ReaderExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.ReaderExecutingAsync(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ReaderExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.ReaderExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ScalarExecuted_ShouldLock()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.ScalarExecuted(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void ScalarExecuted_ReturnsProcessedResult()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.ScalarExecuted(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ScalarExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ScalarExecuted(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task ScalarExecutedAsync_ShouldLock()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.ScalarExecutedAsync(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ScalarExecutedAsync_ReturnsProcessedResult()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.ScalarExecutedAsync(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ScalarExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.ScalarExecutedAsync(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ScalarExecuting_ShouldLock()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        interceptor.ScalarExecuting(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.Lock(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void ScalarExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = interceptor.ScalarExecuting(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ScalarExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ScalarExecuting(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task ScalarExecutingAsync_ShouldLock()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<IDisposable>(new MockDisposable()));

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        await interceptor.ScalarExecutingAsync(command, eventData, result);

        // Assert
        lockProvider.Verify(lp => lp.LockAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ScalarExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await interceptor.ScalarExecutingAsync(command, eventData, result);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task ScalarExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty, result: null, async: false,
            logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero, CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record.ExceptionAsync(async ()
            => await interceptor.ScalarExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.Null(actual);
    }
}