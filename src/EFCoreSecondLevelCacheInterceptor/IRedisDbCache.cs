namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// RedisDb Cache Manager
    /// </summary>
    public interface IRedisDbCache
    {
        /// <summary>
        /// Gets the value associated with the specified key as string.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        string GetValueAsString(string key, EFCachePolicy cachePolicy);

        /// <summary>
        /// Clear all cached data.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        bool Exists(string key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        object Get(string key, EFCachePolicy cachePolicy);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        T Get<T>(string key, EFCachePolicy cachePolicy);

        /// <summary>
        /// Removes an item by key.
        /// </summary>
        /// <param name="key">key</param>
        void Remove(string key);

        /// <summary>
        /// Removes items by the specified pattern.
        /// </summary>
        /// <param name="pattern">pattern</param>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">Data</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        void Set(string key, object value, EFCachePolicy cachePolicy);
    }
}