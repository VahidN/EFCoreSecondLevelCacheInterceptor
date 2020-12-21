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

    /// <summary>
    /// Reader writer locking service
    /// </summary>
    public class ReaderWriterLockProvider : IReaderWriterLockProvider
    {
        private bool _isDisposed;
        private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional integer time-out.
        /// </summary>
        public void TryReadLocked(Action action, int timeout = Timeout.Infinite)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!_readerWriterLock.TryEnterReadLock(timeout))
            {
                throw new TimeoutException();
            }

            try
            {
                action();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to enter the lock in read mode, with an optional integer time-out.
        /// </summary>
        public T TryReadLocked<T>(Func<T> action, int timeout = Timeout.Infinite)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!_readerWriterLock.TryEnterReadLock(timeout))
            {
                throw new TimeoutException();
            }

            try
            {
                return action();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Tries to enter the lock in write mode, with an optional time-out.
        /// </summary>
        public void TryWriteLocked(Action action, int timeout = Timeout.Infinite)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!_readerWriterLock.TryEnterWriteLock(timeout))
            {
                throw new TimeoutException();
            }

            try
            {
                action();
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        _readerWriterLock?.Dispose();
                    }
                }
                finally
                {
                    _isDisposed = true;
                }
            }
        }
    }
}