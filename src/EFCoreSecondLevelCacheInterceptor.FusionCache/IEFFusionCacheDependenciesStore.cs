using System.Collections.Generic;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Provides information about the in-use cache-dependencies
/// </summary>
public interface IEFFusionCacheDependenciesStore
{
    /// <summary>
    ///     Adds the given tags list to the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    void AddCacheDependencies(IEnumerable<string>? tags);

    /// <summary>
    ///     Removes the given tags list from the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    void RemoveCacheDependencies(IEnumerable<string>? tags);

    /// <summary>
    ///     Returns the cached entries added by this library.
    /// </summary>
    ISet<string> GetAllCacheDependencies();
}