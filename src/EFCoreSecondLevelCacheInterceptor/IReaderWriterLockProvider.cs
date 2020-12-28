using System;
using System.Threading;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Reader writer locking service
    /// </summary>
    public interface IReaderWriterLockProvider : IDisposable
    {
        /// <summary>
        /// Tries to enter the lock in read mode, with an optional integer time-out.
        /// </summary>
        void TryReadLocked(Action action, int timeout = Timeout.Infinite);

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional integer time-out.
        /// </summary>
        T TryReadLocked<T>(Func<T> action, int timeout = Timeout.Infinite);

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out.
        /// </summary>
        void TryWriteLocked(Action action, int timeout = Timeout.Infinite);
    }
}