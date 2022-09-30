namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Cache Service Provider Contract.
/// </summary>
public interface IEFCacheServiceProvider
{
    /// <summary>
    ///     Removes the cached entries added by this library.
    /// </summary>
    void ClearAllCachedEntries();

    /// <summary>
    ///     Gets a cached entry by key.
    /// </summary>
    /// <param name="cacheKey">key to find</param>
    /// <returns>cached value</returns>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    EFCachedData GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy);

    /// <summary>
    ///     Adds a new item to the cache.
    /// </summary>
    /// <param name="cacheKey">key</param>
    /// <param name="value">value</param>
    /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
    void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy);

    /// <summary>
    ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
    /// </summary>
    /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
    void InvalidateCacheDependencies(EFCacheKey cacheKey);
}