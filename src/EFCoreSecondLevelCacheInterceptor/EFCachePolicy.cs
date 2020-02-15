using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// EFCachePolicy determines the Expiration time of the cache.
    /// </summary>
    public class EFCachePolicy
    {
        /// <summary>
        /// It's `|`
        /// </summary>
        public const char ItemsSeparator = '|';

        /// <summary>
        /// It's `-->`
        /// </summary>
        public const string PartsSeparator = "-->";

        /// <summary>
        /// It's `_`
        /// </summary>
        public const string CacheDependenciesSeparator = "_";

        /// <summary>
        /// It's an special key for unknown cache dependencies
        /// </summary>
        public const string EFUnknownsCacheDependency = nameof(EFUnknownsCacheDependency);

        /// <summary>
        /// Tells the compiler to insert the name of the containing member instead of a parameter’s default value
        /// </summary>
        public string CacheCallerMemberName { get; private set; }

        /// <summary>
        /// Tells the compiler to insert the line number of the containing member instead of a parameter’s default value
        /// </summary>
        public int CacheCallerLineNumber { get; private set; }

        /// <summary>
        /// Defines the expiration mode of the cache item.
        /// Its default value is Absolute.
        /// </summary>
        public CacheExpirationMode CacheExpirationMode { get; private set; }

        /// <summary>
        /// The expiration timeout.
        /// Its default value is 20 minutes later.
        /// </summary>
        public TimeSpan CacheTimeout { get; private set; } = TimeSpan.FromMinutes(20);

        /// <summary>
        /// If you think the computed hash of the query to calculate the cache-key is not enough, set this value.
        /// Its default value is string.Empty.
        /// </summary>
        public string CacheSaltKey { get; private set; } = string.Empty;

        /// <summary>
        /// Determines which entities are used in this LINQ query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </summary>
        public ISet<string> CacheItemsDependencies { get; private set; } = new SortedSet<string>();

        /// <summary>
        /// Tells the compiler to insert the line number of the containing member instead of a parameter’s default value
        /// </summary>
        public EFCachePolicy CallerLineNumber(int lineNumber)
        {
            CacheCallerLineNumber = lineNumber;
            return this;
        }

        /// <summary>
        /// Tells the compiler to insert the name of the containing member instead of a parameter’s default value
        /// </summary>
        public EFCachePolicy CallerMemberName(string memberName)
        {
            CacheCallerMemberName = memberName;
            return this;
        }

        /// <summary>
        /// Set this option to the `real` related table names of the current query, if you are using an stored procedure,
        /// otherwise cache dependencies of normal queries will be calculated automatically.
        /// `cacheDependencies` determines which tables are used in this final query.
        /// This array will be used to invalidate the related cache of all related queries automatically.
        /// </summary>
        public EFCachePolicy CacheDependencies(params string[] cacheDependencies)
        {
            CacheItemsDependencies = new SortedSet<string>(cacheDependencies);
            return this;
        }

        /// <summary>
        /// Defines the expiration mode of the cache item.
        /// Its default value is Absolute.
        /// </summary>
        public EFCachePolicy ExpirationMode(CacheExpirationMode expirationMode)
        {
            CacheExpirationMode = expirationMode;
            return this;
        }

        /// <summary>
        /// The expiration timeout.
        /// Its default value is 20 minutes later.
        /// </summary>
        public EFCachePolicy Timeout(TimeSpan timeout)
        {
            CacheTimeout = timeout;
            return this;
        }

        /// <summary>
        /// If you think the computed hash of the query to calculate the cache-key is not enough, set this value.
        /// Its default value is string.Empty.
        /// </summary>
        public EFCachePolicy SaltKey(string saltKey)
        {
            CacheSaltKey = saltKey;
            return this;
        }

        /// <summary>
        /// Determines the Expiration time of the cache.
        /// </summary>
        public static string Configure(Action<EFCachePolicy> options)
        {
            var cachePolicy = new EFCachePolicy();
            options.Invoke(cachePolicy);
            return cachePolicy.ToString();
        }

        /// <summary>
        /// Represents the textual form of the current object
        /// </summary>
        public override string ToString()
        {
            return $"{nameof(EFCachePolicy)}[{CacheCallerMemberName}(line {CacheCallerLineNumber})] {PartsSeparator} {CacheExpirationMode}{ItemsSeparator}{CacheTimeout}{ItemsSeparator}{CacheSaltKey}{ItemsSeparator}{string.Join(CacheDependenciesSeparator, CacheItemsDependencies)}".TrimEnd(ItemsSeparator);
        }
    }
}