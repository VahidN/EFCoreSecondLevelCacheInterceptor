using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    public class EFDebugLogger : IEFDebugLogger
    {
        private readonly bool _disableLogging;
        private readonly ILogger<EFDebugLogger> _logger;
        private readonly string _signature = $"InstanceId: {Guid.NewGuid()}, Started @{DateTime.UtcNow} UTC.";

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public EFDebugLogger(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            ILogger<EFDebugLogger> logger)
        {
            if (cacheSettings == null)
            {
                throw new ArgumentNullException(nameof(cacheSettings));
            }

            _disableLogging = cacheSettings.Value.DisableLogging;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public void LogDebug(string message)
        {
            if (!_disableLogging)
            {
                _logger.LogDebug($"{_signature} {message}");
            }
        }

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public void LogDebug(EventId eventId, string message)
        {
            if (!_disableLogging)
            {
                _logger.LogDebug(eventId, $"{_signature} {message}");
            }
        }
    }
}