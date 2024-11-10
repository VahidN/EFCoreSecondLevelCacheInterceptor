using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AsyncKeyedLock;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public sealed class LockProvider : ILockProvider
{
    private readonly AsyncNonKeyedLocker _lock = new();
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(value: 7);

    /// <summary>
    ///     Tries to enter the sync lock
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IDisposable? Lock(CancellationToken cancellationToken = default)
        => _lock.LockOrNull(_timeout, cancellationToken);

    /// <summary>
    ///     Tries to enter the async lock
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<IDisposable?> LockAsync(CancellationToken cancellationToken = default)
        => _lock.LockOrNullAsync(_timeout, cancellationToken);

    /// <summary>
    ///     Disposes the lock
    /// </summary>
    public void Dispose() => _lock.Dispose();
}