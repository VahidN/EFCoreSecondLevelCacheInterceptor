using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Cache Dependencies Calculator
    /// </summary>
    public interface IEFCacheDependenciesProcessor
    {
        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        SortedSet<string> GetCacheDependencies(DbCommand command, DbContext context, EFCachePolicy cachePolicy);

        /// <summary>
        /// Finds the related table names of the current query.
        /// </summary>
        SortedSet<string> GetCacheDependencies(EFCachePolicy cachePolicy, SortedSet<string> tableNames, string commandText);

        /// <summary>
        /// Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        bool InvalidateCacheDependencies(string commandText, EFCacheKey cacheKey);
    }
}