using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Provides information about the currently executing query
/// </summary>
public class EFCachePolicyContext
{
    /// <summary>
    ///     The currently executing query
    /// </summary>
    public string CommandText { set; get; } = null!;

    /// <summary>
    ///     The associated currently executing query's table names.
    /// </summary>
    public ISet<string> CommandTableNames { set; get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The associated currently executing query's entity types.
    /// </summary>
    public IList<Type> CommandEntityTypes { set; get; } = [];

    /// <summary>
    ///     Is `insert`, `update` or `delete`?
    /// </summary>
    public bool IsCrudCommand { set; get; }

    /// <summary>
    ///     The currently executing query's EFCachePolicy value, before being possibly altered by calling
    ///     `OverrideCachePolicy()` method.
    /// </summary>
    public EFCachePolicy? CommandCachePolicy { set; get; }
}