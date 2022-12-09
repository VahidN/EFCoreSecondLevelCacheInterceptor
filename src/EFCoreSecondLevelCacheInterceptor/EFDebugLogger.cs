using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Formats and writes a debug log message.
/// </summary>
public class EFDebugLogger : IEFDebugLogger
{
    /// <summary>
    ///     Formats and writes a debug log message.
    /// </summary>
    public EFDebugLogger(
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        ILogger<EFDebugLogger> logger)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        var disableLogging = cacheSettings.Value.DisableLogging;
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        IsLoggerEnabled = !disableLogging && logger.IsEnabled(LogLevel.Debug);
        if (IsLoggerEnabled)
        {
            logger.LogDebug("InstanceId: {Id}, Started @{Date} UTC.", Guid.NewGuid(), DateTime.UtcNow);
        }
    }

    /// <summary>
    ///     Determines whether the debug logger is enabled.
    /// </summary>
    public bool IsLoggerEnabled { get; }
}