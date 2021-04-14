using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// CacheAllQueries Options
    /// </summary>
    public class SkipCacheSpecificQueriesOptions : CacheAllQueriesOptions
    {
        /// <summary>
        /// Given entity types to cache
        /// </summary>
        public IList<Type>? EntityTypes { get; }

        /// <summary>
        /// Given table names to cache
        /// </summary>
        public IEnumerable<string>? TableNames { set; get; }

        /// <summary>
        /// CacheAllQueries Options
        /// </summary>
        public SkipCacheSpecificQueriesOptions(IList<Type>? entityTypes)
        {
            EntityTypes = entityTypes;
        }
    }
}