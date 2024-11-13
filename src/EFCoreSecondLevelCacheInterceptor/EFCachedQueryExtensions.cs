using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Returns a new cached query.
/// </summary>
public static class EFCachedQueryExtensions
{
    private static readonly TimeSpan _thirtyMinutes = TimeSpan.FromMinutes(value: 30);

    /// <summary>
    ///     IsNotCachable Marker
    /// </summary>
    public static readonly string IsNotCachableMarker =
        $"{nameof(EFCoreSecondLevelCacheInterceptor)}{nameof(NotCacheable)}";

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this IQueryable<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout)
    {
        SanityCheck(query);

        return query.TagWith(
            EFCachePolicy.Configure(options => options.ExpirationMode(expirationMode).Timeout(timeout)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="cacheDependencies">
    ///     Set this option to the `real` related table names of the current query, if you are using an stored procedure,
    ///     otherwise cache dependencies of normal queries will be calculated automatically.
    ///     `cacheDependencies` determines which tables are used in this final query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </param>
    /// <param name="saltKey">
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this
    ///     value.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this IQueryable<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string[] cacheDependencies,
        string saltKey)
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode)
                .Timeout(timeout)
                .CacheDependencies(cacheDependencies)
                .SaltKey(saltKey)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="cacheDependencies">
    ///     Set this option to the `real` related table names of the current query, if you are using an stored procedure,
    ///     otherwise cache dependencies of normal queries will be calculated automatically.
    ///     `cacheDependencies` determines which tables are used in this final query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this IQueryable<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string[] cacheDependencies)
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode).Timeout(timeout).CacheDependencies(cacheDependencies)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="saltKey">
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this
    ///     value.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this IQueryable<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string saltKey)
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode).Timeout(timeout).SaltKey(saltKey)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned by it will be cached only for 30 minutes.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this IQueryable<TType> query)
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(CacheExpirationMode.Absolute)
                .Timeout(_thirtyMinutes)
                .DefaultCacheableMethod(state: true)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned by it will be cached only for 30 minutes.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this DbSet<TType> query)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(CacheExpirationMode.Absolute)
                .Timeout(_thirtyMinutes)
                .DefaultCacheableMethod(state: true)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this DbSet<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(
            EFCachePolicy.Configure(options => options.ExpirationMode(expirationMode).Timeout(timeout)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="cacheDependencies">
    ///     Set this option to the `real` related table names of the current query, if you are using an stored procedure,
    ///     otherwise cache dependencies of normal queries will be calculated automatically.
    ///     `cacheDependencies` determines which tables are used in this final query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </param>
    /// <param name="saltKey">
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this
    ///     value.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this DbSet<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string[] cacheDependencies,
        string saltKey)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode)
                .Timeout(timeout)
                .CacheDependencies(cacheDependencies)
                .SaltKey(saltKey)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="cacheDependencies">
    ///     Set this option to the `real` related table names of the current query, if you are using an stored procedure,
    ///     otherwise cache dependencies of normal queries will be calculated automatically.
    ///     `cacheDependencies` determines which tables are used in this final query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this DbSet<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string[] cacheDependencies)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode).Timeout(timeout).CacheDependencies(cacheDependencies)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
    /// <param name="timeout">The expiration timeout.</param>
    /// <param name="saltKey">
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this
    ///     value.
    /// </param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> Cacheable<TType>(this DbSet<TType> query,
        CacheExpirationMode expirationMode,
        TimeSpan timeout,
        string saltKey)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(EFCachePolicy.Configure(options
            => options.ExpirationMode(expirationMode).Timeout(timeout).SaltKey(saltKey)));
    }

    /// <summary>
    ///     Returns a new query where the entities returned will note be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> NotCacheable<TType>(this IQueryable<TType> query)
    {
        SanityCheck(query);

        return query.TagWith(IsNotCachableMarker);
    }

    /// <summary>
    ///     Returns a new query where the entities returned will note be cached.
    /// </summary>
    /// <typeparam name="TType">Entity type.</typeparam>
    /// <param name="query">The input EF query.</param>
    /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
    public static IQueryable<TType> NotCacheable<TType>(this DbSet<TType> query)
        where TType : class
    {
        SanityCheck(query);

        return query.TagWith(IsNotCachableMarker);
    }

    private static void SanityCheck<TType>(IQueryable<TType> query)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (query.Provider is not EntityQueryProvider)
        {
            Debug.WriteLine(message: "`Cacheable` method is designed only for relational EF Core queries.");
        }
    }
}