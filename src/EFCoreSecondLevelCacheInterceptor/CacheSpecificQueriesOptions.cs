using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     CacheAllQueries Options
/// </summary>
/// <remarks>
///     CacheAllQueries Options
/// </remarks>
public class CacheSpecificQueriesOptions(IList<Type>? entityTypes) : CacheAllQueriesOptions
{
    /// <summary>
    ///     Given entity types to cache
    /// </summary>
    public IList<Type>? EntityTypes { get; } = entityTypes;

    /// <summary>
    ///     How should we determine which tables should be cached?
    /// </summary>
    public TableNameComparison TableNameComparison { set; get; }

    /// <summary>
    ///     How should we determine which tables should be cached?
    /// </summary>
    public TableTypeComparison TableTypeComparison { set; get; }

    /// <summary>
    ///     Given table names to cache
    /// </summary>
    public IEnumerable<string>? TableNames { set; get; }
}