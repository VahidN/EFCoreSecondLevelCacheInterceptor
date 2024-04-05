using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Formats and writes a debug log message.
/// </summary>
public class EFDebugLogger : IEFDebugLogger
{
    private readonly Action<(CacheableLogEventId EventId, string Message)>? _cacheableEvent;

    /// <summary>
    ///     Formats and writes a debug log message.
    /// </summary>
    public EFDebugLogger(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings, ILogger<EFDebugLogger> logger)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var enableLogging = cacheSettings.Value.EnableLogging;
        _cacheableEvent = cacheSettings.Value.CacheableEvent;
        IsLoggerEnabled = enableLogging && (_cacheableEvent is not null || logger.IsEnabled(LogLevel.Debug));

        if (IsLoggerEnabled)
        {
            var message = $"InstanceId: {Guid.NewGuid()}, Started @{DateTime.UtcNow} UTC.";
            logger.LogDebug(message);
            NotifyCacheableEvent(CacheableLogEventId.CachingSystemStarted, message);
        }
    }

    /// <summary>
    ///     Determines whether the debug logger is enabled.
    /// </summary>
    public bool IsLoggerEnabled { get; }

    /// <summary>
    ///     If you set DisableLogging to false, this delegate will give you the internal caching events of the library.
    /// </summary>
    public void NotifyCacheableEvent(CacheableLogEventId eventId, string message)
    {
        if (IsLoggerEnabled && _cacheableEvent is not null)
        {
            _cacheableEvent.Invoke((eventId, message));
        }
    }
}