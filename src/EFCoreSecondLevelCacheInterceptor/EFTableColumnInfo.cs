using System;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// TableColumn's Info
    /// </summary>
    [Serializable]
    public class EFTableColumnInfo
    {
        /// <summary>
        /// The column's ordinal.
        /// </summary>
        public int Ordinal { get; set; }

        /// <summary>
        /// The column's name.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// The column's DbType Name.
        /// </summary>
        public string DbTypeName { get; set; } = default!;

        /// <summary>
        /// The column's Type.
        /// </summary>
        public string TypeName { get; set; } = default!;
    }
}