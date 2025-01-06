using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Provides information about the in-use cache-dependencies
/// </summary>
public class EFHybridCacheDependenciesStore(HybridCache hybridCache) : IEFHybridCacheDependenciesStore
{
    private const string TagsCacheKey = "__EF__HybridCache__Tags__";

    private static readonly HybridCacheEntryOptions Options = new()
    {
        Expiration = TimeSpan.FromDays(value: 1),
        LocalCacheExpiration = TimeSpan.FromDays(value: 1)
    };

    /// <summary>
    ///     Adds the given tags list to the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    public void AddCacheDependencies(IEnumerable<string>? tags)
    {
        tags ??= [];

        // NOTE: hybridCache doesn't have a synchronous API!! So ... we will wait!

        var currentTags = await hybridCache.GetOrCreateAsync<List<string>>(TagsCacheKey,
            factory => ValueTask.FromResult<List<string>>([]));

        foreach (var tag in tags)
        {
            if (!currentTags.Contains(tag, StringComparer.Ordinal))
            {
                currentTags.Add(tag);
            }
        }

        hybridCache.SetAsync(TagsCacheKey, currentTags, Options);
    }

    /// <summary>
    ///     Removes the given tags list from the list of current tags
    /// </summary>
    /// <param name="tags"></param>
    public void RemoveCacheDependencies(IEnumerable<string>? tags)
    {
        tags ??= [];

        var currentTags = await hybridCache.GetOrCreateAsync<List<string>>(TagsCacheKey,
            factory => ValueTask.FromResult<List<string>>([]));

        foreach (var tag in tags)
        {
            currentTags.Remove(tag);
        }

        hybridCache.SetAsync(TagsCacheKey, currentTags, Options);
    }

    /// <summary>
    ///     Returns the cached entries added by this library.
    /// </summary>
    public ISet<string> GetAllCacheDependencies()
        => new HashSet<string>(
            await hybridCache.GetOrCreateAsync<List<string>>(TagsCacheKey,
                factory => ValueTask.FromResult<List<string>>([])), StringComparer.Ordinal);
}