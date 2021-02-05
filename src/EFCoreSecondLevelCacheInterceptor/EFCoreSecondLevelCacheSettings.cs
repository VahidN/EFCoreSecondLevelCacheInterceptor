using System;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Global Cache Settings
    /// </summary>
    public class EFCoreSecondLevelCacheSettings
    {
        /// <summary>
        /// The selected cache provider
        /// </summary>
        public Type CacheProvider { get; set; } = default!;

        /// <summary>
        /// Selected caching provider name
        /// </summary>
        public string ProviderName { get; set; } = default!;

        /// <summary>
        /// Is an instance of EasyCaching.HybridCache
        /// </summary>
        public bool IsHybridCache { get; set; }

        /// <summary>
        /// CacheAllQueries Options
        /// </summary>
        public CacheAllQueriesOptions CacheAllQueriesOptions { get; set; } = new CacheAllQueriesOptions();

        /// <summary>
        /// Cache Specific Queries Options
        /// </summary>
        public CacheSpecificQueriesOptions CacheSpecificQueriesOptions { get; set; } = new CacheSpecificQueriesOptions(entityTypes: null);

        /// <summary>
        /// Should the debug level loggig be disabled?
        /// </summary>
        public bool DisableLogging { set; get; }

        /// <summary>
        /// Here you can decide based on the currect executing SQL command, should we cache its result or not?
        /// </summary>
        public Predicate<string>? SkipCachingCommands { set; get; }

        /// <summary>
        /// Here you can decide based on the currect executing result, should we cache this result or not?
        /// </summary>
        public Predicate<(string CommandText, object Value)>? SkipCachingResults { set; get; }
    }
}