namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Is the configured cache provider online?
/// </summary>
public interface IEFCacheServiceCheck
{
    /// <summary>
    ///     Is the configured cache services online and available? Can we use it without any problem?
    /// </summary>
    bool IsCacheServiceAvailable();
}