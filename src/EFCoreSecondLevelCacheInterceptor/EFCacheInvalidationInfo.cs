using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Represents some information about the current cache invalidation event
/// </summary>
public class EFCacheInvalidationInfo
{
    /// <summary>
    ///     Invalidated all the cache entries due to early expiration of a root cache key
    /// </summary>
    public bool ClearAllCachedEntries { set; get; }

    /// <summary>
    ///     Determines which entities are used in this LINQ query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    ///     If `ClearAllCachedEntries` is set to true, this property will be ignored.
    /// </summary>
    public ISet<string> CacheDependencies { set; get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Defines a mechanism for retrieving a service object.
    ///     For instance, you can create an ILoggerFactory by using it.
    /// </summary>
    public IServiceProvider ServiceProvider { set; get; } = default!;
}