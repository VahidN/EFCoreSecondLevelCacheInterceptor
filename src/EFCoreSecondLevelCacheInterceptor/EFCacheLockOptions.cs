namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Cache Lock Options
/// </summary>
public class EFCacheLockOptions
{
    /// <summary>
    ///     Backward-compatible mode is `Global`. Its default value is `Keyed`.
    ///     Select `None` to opt-out (skip lock) for this provider.
    /// </summary>
    public EFLockMode Mode { get; set; } = EFLockMode.Keyed;

    /// <summary>
    ///     Lock acquisition timeout. Its default value is 7 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(value: 7);
}