using Microsoft.Extensions.Logging;

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
        void LogDebug(string message);

        /// <summary>
        /// Formats and writes a debug log message.
        /// </summary>
        void LogDebug(EventId eventId, string message);
    }
}