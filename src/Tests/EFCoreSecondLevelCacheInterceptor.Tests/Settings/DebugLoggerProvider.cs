using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EFCoreSecondLevelCacheInterceptor.Tests
{
    public sealed class DebugLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, DebugLogger> _loggers = new ConcurrentDictionary<string, DebugLogger>();
        private readonly ConcurrentBag<LogItem> _itemsQueue = new ConcurrentBag<LogItem>();

        public IReadOnlyCollection<LogItem> Items => _itemsQueue;

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, _ => new DebugLogger(_itemsQueue));
        }

        public void Dispose()
        {
        }

        public void ClearItems()
        {
            _itemsQueue.Clear();
        }

        public int GetCacheHitCount()
        {
            var count = _itemsQueue.Count(item => item.EventId == CacheableEventId.CacheHit);
            _itemsQueue.Clear();
            return count;
        }

        public int GetQueryResultsCachedCount()
        {
            return _itemsQueue.Count(item => item.EventId == CacheableEventId.QueryResultCached);
        }
    }

    public class DebugLogger : ILogger
    {
        private readonly ConcurrentBag<LogItem> _itemsQueue;

        public DebugLogger(ConcurrentBag<LogItem> itemsQueue)
        {
            _itemsQueue = itemsQueue;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope<TState>();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _itemsQueue.Add(new LogItem(logLevel, eventId, formatter(state, exception), exception));
        }

        class Scope<TState> : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    public class LogItem
    {
        public LogItem(LogLevel logLevel, EventId eventId, string message, Exception exception)
        {
            DateTime = DateTime.Now;
            LogLevel = logLevel;
            EventId = eventId;
            Message = message;
            Exception = exception;
        }

        public DateTime DateTime { set; get; }
        public LogLevel LogLevel { set; get; }
        public EventId EventId { set; get; }
        public string Message { set; get; }
        public Exception Exception { set; get; }

        public override string ToString()
        {
            return $"{EventId.Id}: {Message}";
        }
    }
}