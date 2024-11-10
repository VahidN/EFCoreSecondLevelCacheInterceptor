using System;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public interface ILockProvider : IDisposable
{
    /// <summary>
    ///     Tries to enter the sync lock
    /// </summary>
    IDisposable? Lock(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to enter the async lock
    /// </summary>
    ValueTask<IDisposable?> LockAsync(CancellationToken cancellationToken = default);
}