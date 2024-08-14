using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Formats and writes a debug log message.
/// </summary>
public interface IEFDebugLogger
{
    /// <summary>
    ///     Determines whether the debug logger is enabled.
    /// </summary>
    bool IsLoggerEnabled { get; }

    /// <summary>
    ///     If you set DisableLogging to false, this delegate will give you the internal caching events of the library.
    /// </summary>
    void NotifyCacheableEvent(CacheableLogEventId eventId, string message, string commandText);

    /// <summary>
    ///     Represents some information about the current cache invalidation event
    /// </summary>
    void NotifyCacheInvalidation(bool clearAllCachedEntries, ISet<string> cacheDependencies);
}