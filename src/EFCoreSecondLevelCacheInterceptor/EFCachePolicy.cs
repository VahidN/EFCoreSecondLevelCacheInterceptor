using System;
using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     EFCachePolicy determines the Expiration time of the cache.
/// </summary>
public class EFCachePolicy
{
    /// <summary>
    ///     It's `|`
    /// </summary>
    public const char ItemsSeparator = '|';

    /// <summary>
    ///     It's `-->`
    /// </summary>
    public const string PartsSeparator = "-->";

    /// <summary>
    ///     It's `_`
    /// </summary>
    public const string CacheDependenciesSeparator = "_";

    /// <summary>
    ///     It's an special key for unknown cache dependencies
    /// </summary>
    public const string UnknownsCacheDependency = nameof(UnknownsCacheDependency);

    /// <summary>
    ///     Defines the expiration mode of the cache item.
    ///     Its default value is Absolute.
    /// </summary>
    public CacheExpirationMode CacheExpirationMode { get; private set; }

    /// <summary>
    ///     The expiration timeout.
    ///     Its default value is 20 minutes later.
    /// </summary>
    public TimeSpan? CacheTimeout { get; private set; } = TimeSpan.FromMinutes(value: 20);

    /// <summary>
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this value.
    ///     Its default value is string.Empty.
    /// </summary>
    public string CacheSaltKey { get; private set; } = string.Empty;

    /// <summary>
    ///     Determines which entities are used in this LINQ query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </summary>
    public ISet<string> CacheItemsDependencies { get; private set; } =
        new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Determines the default Cacheable method
    /// </summary>
    public bool IsDefaultCacheableMethod { set; get; }

    /// <summary>
    ///     Set this option to the `real` related table names of the current query, if you are using an stored procedure,
    ///     otherwise cache dependencies of normal queries will be calculated automatically.
    ///     `cacheDependencies` determines which tables are used in this final query.
    ///     This array will be used to invalidate the related cache of all related queries automatically.
    /// </summary>
    public EFCachePolicy CacheDependencies(params string[] cacheDependencies)
    {
        CacheItemsDependencies = new SortedSet<string>(cacheDependencies, StringComparer.OrdinalIgnoreCase);

        return this;
    }

    /// <summary>
    ///     Defines the expiration mode of the cache item.
    ///     Its default value is Absolute.
    /// </summary>
    public EFCachePolicy ExpirationMode(CacheExpirationMode expirationMode)
    {
        CacheExpirationMode = expirationMode;

        return this;
    }

    /// <summary>
    ///     The expiration timeout.
    ///     Its default value is 20 minutes later.
    /// </summary>
    public EFCachePolicy Timeout(TimeSpan? timeout)
    {
        CacheTimeout = timeout;

        return this;
    }

    /// <summary>
    ///     If you think the computed hash of the query to calculate the cache-key is not enough, set this value.
    ///     Its default value is string.Empty.
    /// </summary>
    public EFCachePolicy SaltKey(string saltKey)
    {
        CacheSaltKey = saltKey;

        return this;
    }

    /// <summary>
    ///     Determines the default Cacheable method
    /// </summary>
    public EFCachePolicy DefaultCacheableMethod(bool state)
    {
        IsDefaultCacheableMethod = state;

        return this;
    }

    /// <summary>
    ///     Determines the Expiration time of the cache.
    /// </summary>
    public static string Configure(Action<EFCachePolicy> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var cachePolicy = new EFCachePolicy();
        options.Invoke(cachePolicy);

        return cachePolicy.ToString();
    }

    /// <summary>
    ///     Represents the textual form of the current object
    /// </summary>
    public override string ToString()
        => $"{nameof(EFCachePolicy)} {PartsSeparator} {CacheExpirationMode}{ItemsSeparator}{CacheTimeout}{ItemsSeparator}{CacheSaltKey}{ItemsSeparator}{string.Join(CacheDependenciesSeparator, CacheItemsDependencies)}{ItemsSeparator}{IsDefaultCacheableMethod}"
            .TrimEnd(ItemsSeparator);
}