using AsyncKeyedLock;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Reader writer locking service
/// </summary>
public sealed class LockProvider : ILockProvider
{
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

    private readonly AsyncNonKeyedLocker _globalLocker = new();
    private readonly AsyncKeyedLocker<string> _keyedLocker = new(StringComparer.Ordinal);

    /// <summary>
    ///     Reader writer locking service
    /// </summary>
    public LockProvider(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheSettings = cacheSettings.Value;
    }

    /// <summary>
    ///     Disposes the lock
    /// </summary>
    public void Dispose()
    {
        _keyedLocker.Dispose();
        _globalLocker.Dispose();
    }

    /// <summary>
    ///     Asynchronously locks and executes the action
    /// </summary>
    public async Task<T> ExecuteWithLockAsync<T>(string lockKey,
        Func<CancellationToken, Task<T>> func,
        CancellationToken cancellationToken = default)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        var lockOptions = _cacheSettings.CacheLockOptions;

        switch (lockOptions.Mode)
        {
            case EFLockMode.None:
                return await func(cancellationToken);

            case EFLockMode.Global:
                using (await _globalLocker.LockOrNullAsync(lockOptions.Timeout, cancellationToken))
                {
                    return await func(cancellationToken);
                }

            case EFLockMode.Keyed:

                if (string.IsNullOrWhiteSpace(lockKey))
                {
                    throw new ArgumentNullException(nameof(lockKey));
                }

                using (await _keyedLocker.LockOrNullAsync(lockKey, lockOptions.Timeout, cancellationToken))
                {
                    return await func(cancellationToken);
                }
            default:
                throw new NotSupportedException($"{lockOptions.Mode} mode is not defined.");
        }
    }

    /// <summary>
    ///     Synchronously locks and executes the action
    /// </summary>
    public T ExecuteWithLock<T>(string lockKey, Func<T> func, CancellationToken cancellationToken = default)
    {
        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        var lockOptions = _cacheSettings.CacheLockOptions;

        switch (lockOptions.Mode)
        {
            case EFLockMode.None:
                return func();

            case EFLockMode.Global:
                using (_globalLocker.LockOrNull(lockOptions.Timeout, cancellationToken))
                {
                    return func();
                }

            case EFLockMode.Keyed:

                if (string.IsNullOrWhiteSpace(lockKey))
                {
                    throw new ArgumentNullException(nameof(lockKey));
                }

                using (_keyedLocker.LockOrNull(lockKey, lockOptions.Timeout, cancellationToken))
                {
                    return func();
                }

            default:
                throw new NotSupportedException($"{lockOptions.Mode} mode is not defined.");
        }
    }
}