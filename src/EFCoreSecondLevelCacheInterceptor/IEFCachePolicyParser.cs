using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// EFCachePolicy Parser Utils
    /// </summary>
    public interface IEFCachePolicyParser
    {
        /// <summary>
        /// Converts the `commandText` to an instance of `EFCachePolicy`
        /// </summary>
        EFCachePolicy? GetEFCachePolicy(string commandText, IList<TableEntityInfo> allEntityTypes);

        /// <summary>
        /// Does `commandText` contain EFCachePolicyTagPrefix?
        /// </summary>
        bool HasEFCachePolicy(string commandText);

        /// <summary>
        /// Removes the EFCachePolicy line from the commandText
        /// </summary>
        string RemoveEFCachePolicyTag(string commandText);
    }
}