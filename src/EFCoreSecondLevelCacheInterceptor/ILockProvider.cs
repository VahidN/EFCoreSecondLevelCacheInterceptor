using System;
using System.Threading;
using System.Threading.Tasks;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public interface ILockProvider
{
    /// <summary>
    ///     Tries to enter the sync lock
    /// </summary>
    IDisposable Lock(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to enter the async lock
    /// </summary>
    Task<IDisposable> LockAsync(CancellationToken cancellationToken = default);
}