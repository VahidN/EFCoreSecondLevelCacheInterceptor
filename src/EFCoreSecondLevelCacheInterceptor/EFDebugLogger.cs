using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Formats and writes a debug log message.
/// </summary>
public class EFDebugLogger : IEFDebugLogger
{
    private readonly Action<EFCacheableLogEvent>? _cacheableEvent;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Formats and writes a debug log message.
    /// </summary>
    public EFDebugLogger(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        ILogger<EFDebugLogger> logger,
        IServiceProvider serviceProvider)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        _serviceProvider = serviceProvider;

        var enableLogging = cacheSettings.Value.EnableLogging;
        _cacheableEvent = cacheSettings.Value.CacheableEvent;
        IsLoggerEnabled = enableLogging && (_cacheableEvent is not null || logger.IsEnabled(LogLevel.Debug));

        if (IsLoggerEnabled)
        {
            var message = $"InstanceId: {Guid.NewGuid()}, Started @{DateTime.UtcNow} UTC.";
            logger.LogDebug(message);
            NotifyCacheableEvent(CacheableLogEventId.CachingSystemStarted, message, commandText: "");
        }
    }

    /// <summary>
    ///     Determines whether the debug logger is enabled.
    /// </summary>
    public bool IsLoggerEnabled { get; }

    /// <summary>
    ///     If you set DisableLogging to false, this delegate will give you the internal caching events of the library.
    /// </summary>
    public void NotifyCacheableEvent(CacheableLogEventId eventId, string message, string commandText)
    {
        if (IsLoggerEnabled && _cacheableEvent is not null)
        {
            _cacheableEvent.Invoke(new EFCacheableLogEvent
            {
                EventId = eventId,
                Message = message,
                CommandText = commandText,
                ServiceProvider = _serviceProvider
            });
        }
    }
}