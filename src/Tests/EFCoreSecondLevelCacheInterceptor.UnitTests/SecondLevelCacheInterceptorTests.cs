using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace EFCoreSecondLevelCacheInterceptor.UnitTests;

[TestClass]
[SuppressMessage(category: "ReSharper", checkId: "AssignNullToNotNullAttribute")]
public class SecondLevelCacheInterceptorTests
{
    [TestMethod]
    public void Constructor_InitializesFields_WhenArgumentsAreValid()
    {
        var processorMock = new Mock<IDbCommandInterceptorProcessor>();

        var interceptor = new SecondLevelCacheInterceptor(processorMock.Object);

        Assert.IsNotNull(interceptor);
    }

    [TestMethod]
    public void NonQueryExecuted_ReturnsProcessedResult()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = interceptor.NonQueryExecuted(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void NonQueryExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual =
            AssertsExtensions.RecordException(()
                => interceptor.NonQueryExecuted(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task NonQueryExecutedAsync_ReturnsProcessedResult()
    {
        // Arrange
        const int expected = int.MaxValue;
        const int result = int.MinValue;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.NonQueryExecutedAsync(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task NonQueryExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        const int expected = 1;
        const int result = 1;

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.NonQueryExecutedAsync(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void NonQueryExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = interceptor.NonQueryExecuting(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void NonQueryExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual =
            AssertsExtensions.RecordException(()
                => interceptor.NonQueryExecuting(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task NonQueryExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.NonQueryExecutingAsync(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task NonQueryExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<int>.SuppressWithResult(int.MaxValue);
        var result = InterceptionResult<int>.SuppressWithResult(int.MinValue);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.NonQueryExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void ReaderExecuted_ReturnsExpectedDbDataReader()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        using var actual = interceptor.ReaderExecuted(command, eventData, expected);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ReaderExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = AssertsExtensions.RecordException(() =>
        {
            using var data = interceptor.ReaderExecuted(command: null, eventData: null, expected);
        });

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task ReaderExecutedAsync_ReturnsExpectedDbDataReader()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p
                => p.ProcessExecutedCommandsAsync(command, eventData.Context, expected, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.ReaderExecutedAsync(command, eventData, expected);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task ReaderExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = Mock.Of<DbDataReader>();
        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, expected)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.ReaderExecutedAsync(command: null, eventData: null, expected));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void ReaderExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = interceptor.ReaderExecuting(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ReaderExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual =
            AssertsExtensions.RecordException(()
                => interceptor.ReaderExecuting(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task ReaderExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.ReaderExecutingAsync(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task ReaderExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var dataReader = Mock.Of<DbDataReader>();
        var expected = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);
        var result = InterceptionResult<DbDataReader>.SuppressWithResult(dataReader);

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.ReaderExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void ScalarExecuted_ReturnsProcessedResult()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = interceptor.ScalarExecuted(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ScalarExecuted_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual =
            AssertsExtensions.RecordException(() => interceptor.ScalarExecuted(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task ScalarExecutedAsync_ReturnsProcessedResult()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.ScalarExecutedAsync(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task ScalarExecutedAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = new object();
        var result = new object();

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutedCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.ScalarExecutedAsync(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public void ScalarExecuting_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = interceptor.ScalarExecuting(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ScalarExecuting_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor.Setup(p => p.ProcessExecutingCommands(command, eventData.Context, result)).Returns(expected);

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual =
            AssertsExtensions.RecordException(()
                => interceptor.ScalarExecuting(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }

    [TestMethod]
    public async Task ScalarExecutingAsync_ReturnsExpectedInterceptionResult()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await interceptor.ScalarExecutingAsync(command, eventData, result);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task ScalarExecutingAsync_ShouldNotThrowArgumentNullException_WhenAnyParameterIsNull()
    {
        // Arrange
        var expected = InterceptionResult<object>.SuppressWithResult(new object());
        var result = InterceptionResult<object>.SuppressWithResult(new object());

        var command = Mock.Of<DbCommand>();

        var eventData = new CommandExecutedEventData(eventDefinition: null, messageGenerator: null, connection: null,
            command, logCommandText: null, context: null, DbCommandMethod.ExecuteNonQuery, Guid.Empty, Guid.Empty,
            result: null, async: false, logParameterValues: false, DateTimeOffset.Now, TimeSpan.Zero,
            CommandSource.LinqQuery);

        var processor = new Mock<IDbCommandInterceptorProcessor>();

        processor
            .Setup(p => p.ProcessExecutingCommandsAsync(command, eventData.Context, result, CancellationToken.None))
            .Returns(Task.FromResult(expected));

        var interceptor = new SecondLevelCacheInterceptor(processor.Object);

        // Act
        var actual = await AssertsExtensions.RecordExceptionAsync(async ()
            => await interceptor.ScalarExecutingAsync(command: null, eventData: null, result));

        // Assert
        Assert.IsNull(actual);
    }
}