using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    public interface IEFDebugLogger
    {
        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        void LogDebug(EventId eventId, string message, params object[] args);
    }

    /// <summary>
    /// Formats and writes a debug log message.
    /// </summary>
    public class EFDebugLogger : IEFDebugLogger
    {
        private readonly bool _disableLogging;
        private readonly ILogger<EFDebugLogger> _logger;

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public EFDebugLogger(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            ILogger<EFDebugLogger> logger)
        {
            _disableLogging = cacheSettings.Value.DisableLogging;
            _logger = logger;
        }

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public void LogDebug(string message, params object[] args)
        {
            if (!_disableLogging)
            {
                _logger.LogDebug(message, args);
            }
        }

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        public void LogDebug(EventId eventId, string message, params object[] args)
        {
            if (!_disableLogging)
            {
                _logger.LogDebug(eventId, message, args);
            }
        }
    }
}