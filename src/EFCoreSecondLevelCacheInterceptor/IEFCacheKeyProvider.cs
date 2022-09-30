using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     A custom cache key provider for EF queries.
/// </summary>
public interface IEFCacheKeyProvider
{
    /// <summary>
    ///     Gets an EF query and returns its hashed key to store in the cache.
    /// </summary>
    /// <param name="command">The EF query.</param>
    /// <param name="context">DbContext is a combination of the Unit Of Work and Repository patterns.</param>
    /// <param name="cachePolicy">determines the Expiration time of the cache.</param>
    /// <returns>Information of the computed key of the input LINQ query.</returns>
    EFCacheKey GetEFCacheKey(DbCommand command, DbContext context, EFCachePolicy cachePolicy);
}