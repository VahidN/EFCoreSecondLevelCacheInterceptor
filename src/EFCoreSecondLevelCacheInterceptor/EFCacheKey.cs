using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Stores information of the computed key of the input LINQ query.
/// </summary>
public class EFCacheKey
{
    /// <summary>
    ///     Stores information of the computed key of the input LINQ query.
    /// </summary>
    public EFCacheKey(ISet<string> cacheDependencies) => CacheDependencies = cacheDependencies;

    /// <summary>
    ///     Hash of the input LINQ query's computed key.
    /// </summary>
    public string KeyHash { set; get; } = default!;

    /// <summary>
    ///     Type of DbContext.
    /// </summary>
    public Type? DbContext { get; set; }

    /// <summary>
    ///     Determines which entities are used in this LINQ query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </summary>
    public ISet<string> CacheDependencies { get; }

    /// <summary>
    ///     Equals
    /// </summary>
    /// <param name="obj"></param>
    public override bool Equals(object? obj)
    {
        if (obj is not EFCacheKey efCacheKey)
        {
            return false;
        }

        return string.Equals(KeyHash, efCacheKey.KeyHash, StringComparison.Ordinal) && DbContext == efCacheKey.DbContext;
    }

    /// <summary>
    ///     GetHashCode
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            return hash * 23 + KeyHash.GetHashCode(StringComparison.Ordinal) + (DbContext == null ? 0 : DbContext.Name.GetHashCode(StringComparison.Ordinal));
        }
    }

    /// <summary>
    ///     ToString
    /// </summary>
    public override string ToString() =>
        $"KeyHash: {KeyHash}, DbContext: {DbContext?.Name}, CacheDependencies: {string.Join(", ", CacheDependencies)}.";
}