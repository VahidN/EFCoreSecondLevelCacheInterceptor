namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     A custom cache key prefix provider for EF queries.
/// </summary>
public interface IEFCacheKeyPrefixProvider
{
    /// <summary>
    ///     returns the current provided cache key prefix
    /// </summary>
    string GetCacheKeyPrefix();
}