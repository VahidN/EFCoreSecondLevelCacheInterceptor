using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using AsyncKeyedLock;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
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

        Assert.Throws<ArgumentNullException>(() => new SecondLevelCacheInterceptor(null, lockProviderMock.Object));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLockProviderIsNull()
    {
        var processorMock = new Mock<IDbCommandInterceptorProcessor>();

        Assert.Throws<ArgumentNullException>(() => new SecondLevelCacheInterceptor(processorMock.Object, null));
    }

    [Fact]
    public void NonQueryExecuted_ShouldLock()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void NonQueryExecuted_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.NonQueryExecuted(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task NonQueryExecutedAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = 1;
        const int result = 1;

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.NonQueryExecutedAsync(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void NonQueryExecuting_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.NonQueryExecuting(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task NonQueryExecutingAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.NonQueryExecutingAsync(null, null, result));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ReaderExecuted_ShouldLock()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void ReaderExecuted_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ReaderExecuted(null, null, expected));

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public async Task ReaderExecutedAsync_ShouldLock()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task ReaderExecutedAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.ReaderExecutedAsync(null, null, expected));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void ReaderExecuting_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ReaderExecuting(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task ReaderExecutingAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.ReaderExecutingAsync(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void ScalarExecuted_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ScalarExecuted(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task ScalarExecutedAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.ScalarExecutedAsync(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public void ScalarExecuting_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = Record.Exception(() => interceptor.ScalarExecuting(null, null, result));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.LockAsync(CancellationToken.None))
            .Returns(new ValueTask<AsyncNonKeyedLockReleaser>(new AsyncNonKeyedLockReleaser()));

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
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

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
    public async Task ScalarExecutingAsync_ShouldNotArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();
        var eventData = new CommandExecutedEventData(
            null,
            null,
            null,
            command,
            null,
            DbCommandMethod.ExecuteNonQuery,
            Guid.Empty,
            Guid.Empty,
            null,
            false,
            false,
            DateTimeOffset.Now,
            TimeSpan.Zero,
            CommandSource.LinqQuery);

        var lockProvider = new Mock<ILockProvider>();
        var processor = new Mock<IDbCommandInterceptorProcessor>();

        lockProvider.Setup(lp => lp.Lock(CancellationToken.None)).Returns(new AsyncNonKeyedLockReleaser());
        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object, lockProvider.Object);

        // Act
        var actual = await Record
            .ExceptionAsync(async () => await interceptor.ScalarExecutingAsync(null, null, result));

        // Assert
        Assert.Null(actual);
    }
}