using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public sealed class DebugLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentBag<LogItem> _itemsQueue = new();
    private readonly ConcurrentDictionary<string, DebugLogger> _loggers = new();

    public IReadOnlyCollection<LogItem> Items => _itemsQueue;

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, _ => new DebugLogger(_itemsQueue));

    public void Dispose()
    {
    }

    public void ClearItems() => _itemsQueue.Clear();

    public int GetCacheHitCount()
    {
        var count = _itemsQueue.Count(item => item.EventId == CacheableEventId.CacheHit);
        _itemsQueue.Clear();

        return count;
    }

    public int GetQueryResultsCachedCount()
        => _itemsQueue.Count(item => item.EventId == CacheableEventId.QueryResultCached);
}