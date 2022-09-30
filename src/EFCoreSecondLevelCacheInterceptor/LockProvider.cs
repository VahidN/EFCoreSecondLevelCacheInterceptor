using System;
using System.Threading;
using System.Threading.Tasks;
using NeoSmart.AsyncLock;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public class LockProvider : ILockProvider
{
    private readonly AsyncLock _lock = new();

    /// <summary>
    ///     Tries to enter the sync lock
    /// </summary>
    public IDisposable Lock(CancellationToken cancellationToken = default) => _lock.Lock(cancellationToken);

    /// <summary>
    ///     Tries to enter the async lock
    /// </summary>
    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default) =>
        _lock.LockAsync(cancellationToken);
}