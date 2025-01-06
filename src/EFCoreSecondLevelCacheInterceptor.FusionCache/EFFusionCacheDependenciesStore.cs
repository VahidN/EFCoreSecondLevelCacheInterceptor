using System;
using System.Collections.Generic;
using ZiggyCreatures.Caching.Fusion;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Provides information about the in-use cache-dependencies
/// </summary>
public class EFFusionCacheDependenciesStore(IFusionCache fusionCache) : IEFFusionCacheDependenciesStore
{
    private const string TagsCacheKey = "__EF__FusionCache__Tags__";

    private static readonly TimeSpan TimeOut = TimeSpan.FromDays(value: 1);

    /// <summary>
    ///     Adds the given tags list to the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    public void AddCacheDependencies(IEnumerable<string>? tags)
    {
        tags ??= [];
        var currentTags = fusionCache.GetOrDefault<List<string>>(TagsCacheKey) ?? [];

        foreach (var tag in tags)
        {
            if (!currentTags.Contains(tag))
            {
                currentTags.Add(tag);
            }
        }

        fusionCache.Set(TagsCacheKey, currentTags, options => { options.SetDuration(TimeOut); });
    }

    /// <summary>
    ///     Removes the given tags list from the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    public void RemoveCacheDependencies(IEnumerable<string>? tags)
    {
        tags ??= [];
        var currentTags = fusionCache.GetOrDefault<List<string>>(TagsCacheKey) ?? [];

        foreach (var tag in tags)
        {
            currentTags.Remove(tag);
        }

        fusionCache.Set(TagsCacheKey, currentTags, options => { options.SetDuration(TimeOut); });
    }

    /// <summary>
    ///     Returns the cached entries added by this library.
    /// </summary>
    public ISet<string> GetAllCacheDependencies()
        => new HashSet<string>(fusionCache.GetOrDefault<List<string>>(TagsCacheKey) ?? [], StringComparer.Ordinal);
}