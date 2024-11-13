using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     CacheAllQueries Options
/// </summary>
/// <remarks>
///     CacheAllQueries Options
/// </remarks>
public class SkipCacheSpecificQueriesOptions(IList<Type>? entityTypes) : CacheAllQueriesOptions
{
    /// <summary>
    ///     Given entity types to cache
    /// </summary>
    public IList<Type>? EntityTypes { get; } = entityTypes;

    /// <summary>
    ///     Given table names to cache
    /// </summary>
    public IEnumerable<string>? TableNames { set; get; }
}