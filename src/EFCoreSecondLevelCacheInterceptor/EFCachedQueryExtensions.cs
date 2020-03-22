using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Returns a new cached query.
    /// </summary>
    public static class EFCachedQueryExtensions
    {
        private static readonly TimeSpan _thirtyMinutes = TimeSpan.FromMinutes(30);

        private static readonly MethodInfo _asNoTrackingMethodInfo =
            typeof(EntityFrameworkQueryableExtensions).GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking));

        /// <summary>
        /// IsNotCachable Marker
        /// </summary>
        public static readonly string IsNotCachableMarker = $"{nameof(EFCoreSecondLevelCacheInterceptor)}{nameof(NotCacheable)}";

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this IQueryable<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout).CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will note be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> NotCacheable<TType>(this IQueryable<TType> query)
        {
            sanityCheck(query);
            return query.TagWith(IsNotCachableMarker);
        }

        /// <summary>
        /// Returns a new query where the entities returned will note be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> NotCacheable<TType>(this DbSet<TType> query) where TType : class
        {
            sanityCheck(query);
            return query.TagWith(IsNotCachableMarker);
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="cacheDependencies">
        /// Set this option to the `real` related table names of the current query, if you are using an stored procedure,
        /// otherswise cache dependencies of normal queries will be calculated automatically.
        /// `cacheDependencies` determines which tables are used in this final query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </param>
        /// <param name="saltKey">If you think the computed hash of the query to calculate the cache-key is not enough, set this value.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this IQueryable<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string[] cacheDependencies, string saltKey, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout)
                .CacheDependencies(cacheDependencies).SaltKey(saltKey)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="cacheDependencies">
        /// Set this option to the `real` related table names of the current query, if you are using an stored procedure,
        /// otherswise cache dependencies of normal queries will be calculated automatically.
        /// `cacheDependencies` determines which tables are used in this final query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this IQueryable<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string[] cacheDependencies, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout).CacheDependencies(cacheDependencies)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="saltKey">If you think the computed hash of the query to calculate the cache-key is not enough, set this value.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this IQueryable<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string saltKey, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout).SaltKey(saltKey)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned by it will be cached only for 30 minutes.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this IQueryable<TType> query, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0)
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(CacheExpirationMode.Absolute).Timeout(_thirtyMinutes).DefaultCacheableMethod(true).CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned by it will be cached only for 30 minutes.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this DbSet<TType> query, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0) where TType : class
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(CacheExpirationMode.Absolute).Timeout(_thirtyMinutes).DefaultCacheableMethod(true).CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this DbSet<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0) where TType : class
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="cacheDependencies">
        /// Set this option to the `real` related table names of the current query, if you are using an stored procedure,
        /// otherswise cache dependencies of normal queries will be calculated automatically.
        /// `cacheDependencies` determines which tables are used in this final query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </param>
        /// <param name="saltKey">If you think the computed hash of the query to calculate the cache-key is not enough, set this value.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this DbSet<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string[] cacheDependencies, string saltKey, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0) where TType : class
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout)
                .CacheDependencies(cacheDependencies).SaltKey(saltKey)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="cacheDependencies">
        /// Set this option to the `real` related table names of the current query, if you are using an stored procedure,
        /// otherswise cache dependencies of normal queries will be calculated automatically.
        /// `cacheDependencies` determines which tables are used in this final query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this DbSet<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string[] cacheDependencies, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0) where TType : class
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout).CacheDependencies(cacheDependencies)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        /// <summary>
        /// Returns a new query where the entities returned will be cached.
        /// </summary>
        /// <typeparam name="TType">Entity type.</typeparam>
        /// <param name="query">The input EF query.</param>
        /// <param name="expirationMode">Defines the expiration mode of the cache item.</param>
        /// <param name="timeout">The expiration timeout.</param>
        /// <param name="saltKey">If you think the computed hash of the query to calculate the cache-key is not enough, set this value.</param>
        /// <param name="methodName">Tells the compiler to insert the name of the containing member instead of a parameter’s default value</param>
        /// <param name="lineNumber">Tells the compiler to insert the line number of the containing member instead of a parameter’s default value</param>
        /// <returns>Provides functionality to evaluate queries against a specific data source.</returns>
        public static IQueryable<TType> Cacheable<TType>(
            this DbSet<TType> query, CacheExpirationMode expirationMode, TimeSpan timeout, string saltKey, [CallerMemberName] string methodName = null, [CallerLineNumber] int lineNumber = 0) where TType : class
        {
            sanityCheck(query);
            return query.markAsNoTracking().TagWith(EFCachePolicy.Configure(options =>
                options.ExpirationMode(expirationMode).Timeout(timeout).SaltKey(saltKey)
                .CallerMemberName(methodName).CallerLineNumber(lineNumber)));
        }

        private static void sanityCheck<TType>(IQueryable<TType> query)
        {
            if (!(query.Provider is EntityQueryProvider))
            {
                throw new NotSupportedException("`Cacheable` method is designed only for relational EF Core queries.");
            }
        }

        private static IQueryable<TType> markAsNoTracking<TType>(this IQueryable<TType> query)
        {
            if (typeof(TType).GetTypeInfo().IsClass)
            {
                return query.Provider.CreateQuery<TType>(
                    Expression.Call(null, _asNoTrackingMethodInfo.MakeGenericMethod(typeof(TType)), query.Expression));
            }
            return query;
        }
    }
}