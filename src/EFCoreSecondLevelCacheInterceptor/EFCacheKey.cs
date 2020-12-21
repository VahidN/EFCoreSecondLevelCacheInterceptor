using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Stores information of the computed key of the input LINQ query.
    /// </summary>
    public class EFCacheKey
    {
        /// <summary>
        /// The computed key of the input LINQ query.
        /// </summary>
        public string Key { set; get; } = default!;

        /// <summary>
        /// Hash of the input LINQ query's computed key.
        /// </summary>
        public string KeyHash { set; get; } = default!;

        /// <summary>
        /// Determines which entities are used in this LINQ query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </summary>
        public ISet<string> CacheDependencies { get; } = new HashSet<string>();

        /// <summary>
        /// Stores information of the computed key of the input LINQ query.
        /// </summary>
        public EFCacheKey(ISet<string> cacheDependencies)
        {
            CacheDependencies = cacheDependencies;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        public override bool Equals(object? obj)
        {
            if (!(obj is EFCacheKey efCacheKey))
                return false;

            return this.KeyHash == efCacheKey.KeyHash;
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                return (hash * 23) + KeyHash.GetHashCode(StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"KeyHash: {KeyHash}, CacheDependencies: {string.Join(", ", CacheDependencies)}.";
        }
    }
}