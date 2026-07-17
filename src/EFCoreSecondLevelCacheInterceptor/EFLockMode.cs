namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Different lock modes
/// </summary>
public enum EFLockMode
{
    /// <summary>
    ///     no locking
    /// </summary>
    None,

    /// <summary>
    ///     single global lock (backwards-compatible)
    /// </summary>
    Global,

    /// <summary>
    ///     keyed by cache key using AsyncKeyedLock
    /// </summary>
    Keyed
}