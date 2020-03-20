using StackExchange.Redis;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// RedisDb Cache Manager
    /// </summary>
    public class RedisDbCache : IRedisDbCache
    {
        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _database;
        private readonly IRedisSerializationProvider _serializationProvider;

        /// <summary>
        /// RedisDb Cache Manager
        /// </summary>
        public RedisDbCache(IConnectionMultiplexer connection, IRedisSerializationProvider serializationProvider)
        {
            _connection = connection;
            _database = _connection.GetDatabase();
            _serializationProvider = serializationProvider;
        }

        /// <summary>
        /// Gets the value associated with the specified key as string.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public string GetValueAsString(string key, EFCachePolicy cachePolicy)
        {
            var value = _database.StringGet(key);
            refreshCacheTimeout(key, value, cachePolicy);
            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public object Get(string key, EFCachePolicy cachePolicy)
        {
            var value = _database.StringGet(key);
            refreshCacheTimeout(key, value, cachePolicy);
            return value.HasValue ? _serializationProvider.Deserialize(value) : null;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public T Get<T>(string key, EFCachePolicy cachePolicy)
        {
            var value = _database.StringGet(key);
            refreshCacheTimeout(key, value, cachePolicy);
            return value.HasValue ? _serializationProvider.Deserialize<T>(value) : default;
        }

        /// <summary>
        /// Adds the specified key and object to the cache.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">Data</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public void Set(string key, object value, EFCachePolicy cachePolicy)
        {
            var entryBytes = _serializationProvider.Serialize(value);
            _database.StringSet(key, entryBytes, cachePolicy.CacheTimeout);
        }

        /// <summary>
        /// Gets a value indicating whether the value associated with the specified key is cached
        /// </summary>
        /// <param name="key">key</param>
        public bool Exists(string key)
        {
            return _database.KeyExists(key);
        }

        /// <summary>
        /// Removes an item by key.
        /// </summary>
        /// <param name="key">key</param>
        public void Remove(string key)
        {
            _database.KeyDelete(key);
        }

        /// <summary>
        /// Removes items by the specified pattern.
        /// </summary>
        /// <param name="pattern">pattern</param>
        public void RemoveByPattern(string pattern)
        {
            clearItems(pattern);
        }

        /// <summary>
        /// Clear all cached data.
        /// </summary>
        public void Clear()
        {
            clearItems();
        }

        private void clearItems(string pattern = "")
        {
            foreach (var endPoint in _connection.GetEndPoints())
            {
                var server = _connection.GetServer(endPoint);
                if (!server.IsConnected)
                {
                    continue;
                }

                var keys = string.IsNullOrWhiteSpace(pattern) ?
                                server.Keys(_database.Database) :
                                server.Keys(_database.Database, pattern: $"*{pattern}*");
                foreach (var key in keys)
                {
                    _database.KeyDelete(key);
                }
            }
        }

        private void refreshCacheTimeout(string key, RedisValue value, EFCachePolicy cachePolicy)
        {
            if (value.HasValue && cachePolicy.CacheExpirationMode == CacheExpirationMode.Sliding)
            {
                _database.KeyExpire(key, cachePolicy.CacheTimeout);
            }
        }
    }
}