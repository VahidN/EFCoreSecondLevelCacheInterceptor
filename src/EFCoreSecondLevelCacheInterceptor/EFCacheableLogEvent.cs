using System;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Provides information about the current logged event
/// </summary>
public class EFCacheableLogEvent
{
    /// <summary>
    ///     Event IDs of the internal logged messages of the library
    /// </summary>
    public CacheableLogEventId EventId { set; get; }

    /// <summary>
    ///     The provides logged message
    /// </summary>
    public string Message { set; get; } = default!;

    /// <summary>
    ///     The related SQL Command
    /// </summary>
    public string CommandText { set; get; } = default!;

    /// <summary>
    ///     Defines a mechanism for retrieving a service object.
    ///     For instance, you can create an ILoggerFactory by using it.
    /// </summary>
    public IServiceProvider ServiceProvider { set; get; } = default!;
}