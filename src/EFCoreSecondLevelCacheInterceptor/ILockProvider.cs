namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public interface ILockProvider : IDisposable
{
    /// <summary>
    ///     Asynchronously locks and executes the action
    /// </summary>
    Task<T> ExecuteWithLockAsync<T>(string lockKey,
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Synchronously locks and executes the action
    /// </summary>
    T ExecuteWithLock<T>(string lockKey, Func<T> func, CancellationToken cancellationToken = default);
}