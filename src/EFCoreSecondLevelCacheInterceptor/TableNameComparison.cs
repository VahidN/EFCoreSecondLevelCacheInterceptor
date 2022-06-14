namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    ///     How should we determine which tables should be cached?
    /// </summary>
    public enum TableNameComparison
    {
        /// <summary>
        ///     Caches queries containing table names having the specified names.
        /// </summary>
        Contains,

        /// <summary>
        ///     Caches queries containing table names not having the specified names.
        /// </summary>
        DoesNotContain,

        /// <summary>
        ///     Caches queries containing table names end with the specified names.
        /// </summary>
        EndsWith,

        /// <summary>
        ///     Caches queries containing table names which do not end with the specified names.
        /// </summary>
        DoesNotEndWith,

        /// <summary>
        ///     Caches queries containing table names start with the specified names.
        /// </summary>
        StartsWith,

        /// <summary>
        ///     Caches queries containing table names which do not start with the specified names.
        /// </summary>
        DoesNotStartWith,

        /// <summary>
        ///     Caches queries containing table names equal to the specified names exclusively.
        /// </summary>
        ContainsOnly,

        /// <summary>
        ///     Caches queries containing table names equal to every one of the specified names exclusively.
        /// </summary>
        ContainsEvery,

        /// <summary>
        ///     Caches queries containing table names not equal to every one of the specified names exclusively.
        /// </summary>
        DoesNotContainEvery
    }
}