using Microsoft.Extensions.Logging;

namespace EFCoreSecondLevelCacheInterceptor.Tests;

public class LogItem(LogLevel logLevel, EventId eventId, string message, Exception? exception)
{
    public DateTime DateTime { set; get; } = DateTime.Now;

    public LogLevel LogLevel { set; get; } = logLevel;

    public EventId EventId { set; get; } = eventId;

    public string Message { set; get; } = message;

    public Exception? Exception { set; get; } = exception;

    public override string ToString() => $"{EventId.Id}: {Message}";
}