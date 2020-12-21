using System;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Cached Data
    /// </summary>
    [Serializable]
    public class EFCachedData
    {
        /// <summary>
        /// DbDataReader's result.
        /// </summary>
        public EFTableRows? TableRows { get; set; }

        /// <summary>
        /// DbDataReader's NonQuery result.
        /// </summary>
        public int NonQuery { get; set; }

        /// <summary>
        /// DbDataReader's Scalar result.
        /// </summary>
        public object? Scalar { get; set; }

        /// <summary>
        /// Is result of the query null?
        /// </summary>
        public bool IsNull { get; set; }
    }
}