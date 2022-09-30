using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     CacheAllQueries Options
/// </summary>
public class CacheSpecificQueriesOptions : CacheAllQueriesOptions
{
    /// <summary>
    ///     CacheAllQueries Options
    /// </summary>
    public CacheSpecificQueriesOptions(IList<Type>? entityTypes) => EntityTypes = entityTypes;

    /// <summary>
    ///     Given entity types to cache
    /// </summary>
    public IList<Type>? EntityTypes { get; }

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