using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public class DebugLogger(ConcurrentBag<LogItem> itemsQueue) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => new Scope();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
        => itemsQueue.Add(new LogItem(logLevel, eventId, formatter(state, exception), exception));

    private sealed class Scope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}