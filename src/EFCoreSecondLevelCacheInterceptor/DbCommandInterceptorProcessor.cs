using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Helps process SecondLevelCacheInterceptor
/// </summary>
public class DbCommandInterceptorProcessor : IDbCommandInterceptorProcessor
{
    private readonly IEFCacheDependenciesProcessor _cacheDependenciesProcessor;
    private readonly IEFCacheKeyProvider _cacheKeyProvider;
    private readonly IEFCacheServiceProvider _cacheService;
    private readonly IEFCacheServiceCheck _cacheServiceCheck;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly IDbCommandIgnoreCachingProcessor _ignoreCachingProcessor;
    private readonly ILogger<DbCommandInterceptorProcessor> _interceptorProcessorLogger;
    private readonly ILockProvider _lockProvider;
    private readonly IEFDebugLogger _logger;

    /// <summary>
    ///     Helps process SecondLevelCacheInterceptor
    /// </summary>
    public DbCommandInterceptorProcessor(IEFDebugLogger logger,
        ILogger<DbCommandInterceptorProcessor> interceptorProcessorLogger,
        IEFCacheServiceProvider cacheService,
        IEFCacheDependenciesProcessor cacheDependenciesProcessor,
        IEFCacheKeyProvider cacheKeyProvider,
        IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IEFCacheServiceCheck cacheServiceCheck,
        IDbCommandIgnoreCachingProcessor ignoreCachingProcessor,
        ILockProvider lockProvider)
    {
        _cacheService = cacheService;
        _cacheDependenciesProcessor = cacheDependenciesProcessor;
        _cacheKeyProvider = cacheKeyProvider;
        _logger = logger;
        _interceptorProcessorLogger = interceptorProcessorLogger;
        _cacheServiceCheck = cacheServiceCheck;
        _ignoreCachingProcessor = ignoreCachingProcessor;
        _lockProvider = lockProvider;

        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheSettings = cacheSettings.Value;
    }

    /// <inheritdoc />
    public async Task<T> ProcessExecutedCommandsAsync<T>(DbCommand command,
        DbContext? context,
        T result,
        CancellationToken cancellationToken)
    {
        if (context is null || command is null)
        {
            return result;
        }

        EFCacheKey? efCacheKey = null;

        try
        {
            if (result is EFTableRowsDataReader rowsReader)
            {
                LogReturningCachedTableRows(command, rowsReader, efCacheKey);

                return result;
            }

            var (success, cachePolicy, cacheKey) = await _lockProvider.ExecuteWithLockAsync(
                $"{nameof(ProcessExecutedCommandsAsync)}{typeof(T).FullName}",
                _ => Task.FromResult(TryGetCacheKey(command, context)), cancellationToken);

            if (!success)
            {
                return result;
            }

            efCacheKey = cacheKey;

            return await _lockProvider.ExecuteWithLockAsync(efCacheKey!.KeyHash,
                _ => Task.FromResult(InsertDataToCache(command, result, efCacheKey, cachePolicy)), cancellationToken);
        }
        catch (Exception ex)
        {
            if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
            {
                throw;
            }

            LogInterceptorError(command, ex, efCacheKey);

            return result;
        }
    }

    /// <summary>
    ///     Reads data from cache or cache it and then returns the result
    /// </summary>
    public T ProcessExecutedCommands<T>(DbCommand command, DbContext? context, T result)
    {
        if (context is null || command is null)
        {
            return result;
        }

        EFCacheKey? efCacheKey = null;

        try
        {
            if (result is EFTableRowsDataReader rowsReader)
            {
                LogReturningCachedTableRows(command, rowsReader, efCacheKey);

                return result;
            }

            var (success, cachePolicy, cacheKey) = _lockProvider.ExecuteWithLock(
                $"{nameof(ProcessExecutedCommands)}{typeof(T).FullName}", () => TryGetCacheKey(command, context));

            if (!success)
            {
                return result;
            }

            efCacheKey = cacheKey;

            return _lockProvider.ExecuteWithLock(efCacheKey!.KeyHash,
                () => InsertDataToCache(command, result, efCacheKey, cachePolicy));
        }
        catch (Exception ex)
        {
            if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
            {
                throw;
            }

            LogInterceptorError(command, ex, efCacheKey);

            return result;
        }
    }

    /// <inheritdoc />
    public async Task<T> ProcessExecutingCommandsAsync<T>(DbCommand command,
        DbContext? context,
        T result,
        CancellationToken cancellationToken)
    {
        if (context is null || command is null)
        {
            return result;
        }

        EFCacheKey? efCacheKey = null;

        try
        {
            var (success, cacheKey, cacheResult) = await _lockProvider.ExecuteWithLockAsync(
                $"{nameof(ProcessExecutingCommandsAsync)}{typeof(T).FullName}",
                _ => Task.FromResult(TryGetCachedResult(command, context)), cancellationToken);

            if (!success)
            {
                return result;
            }

            efCacheKey = cacheKey;

            return await _lockProvider.ExecuteWithLockAsync(efCacheKey!.KeyHash,
                _ => Task.FromResult(TryLoadResultsFromCache(command, result, cacheResult, efCacheKey)),
                cancellationToken);
        }
        catch (Exception ex)
        {
            if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
            {
                throw;
            }

            LogInterceptorError(command, ex, efCacheKey);

            return result;
        }
    }

    /// <summary>
    ///     Reads command's data from the cache, if any.
    /// </summary>
    public T ProcessExecutingCommands<T>(DbCommand command, DbContext? context, T result)
    {
        if (context is null || command is null)
        {
            return result;
        }

        EFCacheKey? efCacheKey = null;

        try
        {
            var (success, cacheKey, cacheResult) = _lockProvider.ExecuteWithLock(
                $"{nameof(ProcessExecutingCommands)}{typeof(T).FullName}", () => TryGetCachedResult(command, context));

            if (!success)
            {
                return result;
            }

            efCacheKey = cacheKey;

            return _lockProvider.ExecuteWithLock(efCacheKey!.KeyHash,
                () => TryLoadResultsFromCache(command, result, cacheResult, efCacheKey));
        }
        catch (Exception ex)
        {
            if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
            {
                throw;
            }

            LogInterceptorError(command, ex, efCacheKey);

            return result;
        }
    }

    private T InsertDataToCache<T>(DbCommand command, T result, EFCacheKey? efCacheKey, EFCachePolicy? cachePolicy)
        => result switch
        {
            int data => TryInsertIntData<T>(result, command.CommandText, data, efCacheKey!, cachePolicy!),
            DbDataReader dataReader => TryInsertTableRows<T>(dataReader, command.CommandText, efCacheKey!,
                cachePolicy!),
            not null => TryInsertObject(result, command.CommandText, efCacheKey!, cachePolicy!),
            _ => result
        };

    private T TryLoadResultsFromCache<T>(DbCommand command, T result, EFCachedData? cacheResult, EFCacheKey? efCacheKey)
        => result switch
        {
            InterceptionResult<DbDataReader> when cacheResult!.IsNull || cacheResult.TableRows == null =>
                TryLoadEmptyTableRows<T>(command.CommandText, efCacheKey!),
            InterceptionResult<DbDataReader> => TryLoadCacheResults<T>(cacheResult, efCacheKey!, command.CommandText),
            InterceptionResult<int> => TryLoadIntResultFromCache<T>(cacheResult!, efCacheKey!, command.CommandText),
            InterceptionResult<object> => TryLoadObjectResultFromCache<T>(cacheResult!, efCacheKey!,
                command.CommandText),
            _ => LogSkippedResult(result, command.CommandText, efCacheKey!)
        };

    private (bool Success, EFCacheKey? efCacheKey, EFCachedData? cacheResult) TryGetCachedResult(DbCommand command,
        DbContext context)
    {
        if (!_cacheServiceCheck.IsCacheServiceAvailable())
        {
            return (false, null, null);
        }

        var (shouldSkipProcessing, cachePolicy) = _ignoreCachingProcessor.ShouldSkipProcessing(command, context);

        if (shouldSkipProcessing)
        {
            return (false, null, null);
        }

        if (cachePolicy == null)
        {
            LogSkippingNoneCachableCommand(command.CommandText, efCacheKey: null);

            return (false, null, null);
        }

        var efCacheKey = _cacheKeyProvider.GetEFCacheKey(command, context, cachePolicy);

        if (_cacheService.GetValue(efCacheKey, cachePolicy) is not { } cacheResult)
        {
            LogCacheMiss(efCacheKey);

            return (false, null, null);
        }

        return (true, efCacheKey, cacheResult);
    }

    private (bool Success, EFCachePolicy? cachePolicy, EFCacheKey? efCacheKey) TryGetCacheKey(DbCommand command,
        DbContext context)
    {
        var (process, cachePolicy) = ShouldProcessExecutedCommands(command, context);

        if (!process)
        {
            return (false, null, null);
        }

        var efCacheKey = _cacheKeyProvider.GetEFCacheKey(command, context, cachePolicy ?? new EFCachePolicy());

        if (_cacheDependenciesProcessor.InvalidateCacheDependencies(command.CommandText, efCacheKey))
        {
            return (false, null, null);
        }

        if (cachePolicy == null)
        {
            LogSkippingNoneCachableCommand(command.CommandText, efCacheKey);

            return (false, null, null);
        }

        return (true, cachePolicy, efCacheKey);
    }

    private T TryLoadObjectResultFromCache<T>(EFCachedData cacheResult, EFCacheKey efCacheKey, string commandText)
    {
        var cachedResult = cacheResult.IsNull ? null : cacheResult.Scalar;

        LogSuppressedObjectResultFromCache(cachedResult, efCacheKey, commandText);

        return (T)Convert.ChangeType(InterceptionResult<object>.SuppressWithResult(cachedResult ?? new object()),
            typeof(T), CultureInfo.InvariantCulture);
    }

    private T TryLoadIntResultFromCache<T>(EFCachedData cacheResult, EFCacheKey efCacheKey, string commandText)
    {
        var cachedResult = cacheResult.IsNull ? 0 : cacheResult.NonQuery;

        LogSuppressedIntResultFromCache(cachedResult, efCacheKey, commandText);

        return (T)Convert.ChangeType(InterceptionResult<int>.SuppressWithResult(cachedResult), typeof(T),
            CultureInfo.InvariantCulture);
    }

    private T TryLoadCacheResults<T>(EFCachedData cacheResult, EFCacheKey efCacheKey, string commandText)
    {
        LogSuppressedWithCacheResults(cacheResult, efCacheKey, commandText);

        using var dataRows = new EFTableRowsDataReader(cacheResult.TableRows!
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0
            , _cacheSettings
#endif
        );

        return (T)Convert.ChangeType(InterceptionResult<DbDataReader>.SuppressWithResult(dataRows), typeof(T),
            CultureInfo.InvariantCulture);
    }

    private T TryLoadEmptyTableRows<T>(string commandText, EFCacheKey efCacheKey)
    {
        LogSuppressedWithEmptyTableRows(commandText, efCacheKey);

        using var rows = new EFTableRowsDataReader(new EFTableRows()
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0
            , _cacheSettings
#endif
        );

        return (T)Convert.ChangeType(InterceptionResult<DbDataReader>.SuppressWithResult(rows), typeof(T),
            CultureInfo.InvariantCulture);
    }

    private T TryInsertObject<T>(T result, string commandText, EFCacheKey efCacheKey, EFCachePolicy cachePolicy)
    {
        if (_ignoreCachingProcessor.ShouldSkipCachingResults(commandText, result))
        {
            return result;
        }

        _cacheService.InsertValue(efCacheKey, new EFCachedData
        {
            Scalar = result
        }, cachePolicy);

        LogObjectAddedToCache(result, efCacheKey, commandText);

        return result;
    }

    private T TryInsertTableRows<T>(DbDataReader dataReader,
        string commandText,
        EFCacheKey efCacheKey,
        EFCachePolicy cachePolicy)
    {
        EFTableRows tableRows;

        using (var dbReaderLoader = new EFDataReaderLoader(dataReader))
        {
            tableRows = dbReaderLoader.Load();
        }

        if (_ignoreCachingProcessor.ShouldSkipCachingResults(commandText, tableRows))
        {
            return (T)(object)new EFTableRowsDataReader(tableRows
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0
                , _cacheSettings
#endif
            );
        }

        _cacheService.InsertValue(efCacheKey, new EFCachedData
        {
            TableRows = tableRows
        }, cachePolicy);

        LogTableRowsAddedToCache(tableRows, efCacheKey, commandText);

        return (T)(object)new EFTableRowsDataReader(tableRows
#if NET10_0 || NET9_0 || NET8_0 || NET7_0 || NET6_0 || NET5_0
            , _cacheSettings
#endif
        );
    }

    private T TryInsertIntData<T>(T result,
        string commandText,
        int data,
        EFCacheKey efCacheKey,
        EFCachePolicy cachePolicy)
    {
        if (_ignoreCachingProcessor.ShouldSkipCachingResults(commandText, data))
        {
            return result;
        }

        _cacheService.InsertValue(efCacheKey, new EFCachedData
        {
            NonQuery = data
        }, cachePolicy);

        LogIntAddedToCache(data, efCacheKey, commandText);

        return result;
    }

    private T LogSkippedResult<T>(T result, string commandText, EFCacheKey efCacheKey)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return result;
        }

        var message = $"Skipped the result with {result?.GetType()} type.";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText, efCacheKey);

        return result;
    }

    private void LogSuppressedObjectResultFromCache(object? cachedResult, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"Suppressed the result with {cachedResult} from the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultSuppressed, message, commandText, efCacheKey);
    }

    private void LogSuppressedIntResultFromCache(int cachedResult, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"Suppressed the result with {cachedResult} from the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultSuppressed, message, commandText, efCacheKey);
    }

    private void LogSuppressedWithCacheResults(EFCachedData cacheResult, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message =
            $"Suppressed the result with the TableRows[{cacheResult.TableRows?.TableName}] from the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultSuppressed, message, commandText, efCacheKey);
    }

    private void LogSuppressedWithEmptyTableRows(string commandText, EFCacheKey efCacheKey)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = "Suppressed the result with an empty TableRows.";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultSuppressed, message, commandText, efCacheKey);
    }

    private void LogCacheMiss(EFCacheKey efCacheKey)
    {
        if (_logger.IsLoggerEnabled && _interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message: "[{EfCacheKey}] was not present in the cache.", efCacheKey);
        }
    }

    private void LogInterceptorError(DbCommand command, Exception ex, EFCacheKey? efCacheKey)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        _interceptorProcessorLogger.LogCritical(ex, message: "Interceptor Error");

        _logger.NotifyCacheableEvent(CacheableLogEventId.CachingError, ex.ToString(), command.CommandText, efCacheKey);
    }

    private void LogObjectAddedToCache<T>(T result, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"[{result}] added to the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(CacheableEventId.QueryResultCached, message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultCached, message, commandText, efCacheKey);
    }

    private void LogTableRowsAddedToCache(EFTableRows tableRows, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"TableRows[{tableRows.TableName}] added to the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(CacheableEventId.QueryResultCached, message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultCached, message, commandText, efCacheKey);
    }

    private void LogIntAddedToCache(int data, EFCacheKey efCacheKey, string commandText)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"[{data}] added to the cache[{efCacheKey}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(CacheableEventId.QueryResultCached, message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.QueryResultCached, message, commandText, efCacheKey);
    }

    private void LogSkippingNoneCachableCommand(string commandText, EFCacheKey? efCacheKey)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var message = $"Skipping a none-cachable command[{commandText}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(message);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText, efCacheKey);
    }

    private void LogReturningCachedTableRows(DbCommand command,
        EFTableRowsDataReader rowsReader,
        EFCacheKey? efCacheKey)
    {
        if (!_logger.IsLoggerEnabled)
        {
            return;
        }

        var logMessage = $"Returning the cached TableRows[{rowsReader.TableName}].";

        if (_interceptorProcessorLogger.IsEnabled(LogLevel.Debug))
        {
            _interceptorProcessorLogger.LogDebug(CacheableEventId.CacheHit, logMessage);
        }

        _logger.NotifyCacheableEvent(CacheableLogEventId.CacheHit, logMessage, command.CommandText, efCacheKey);
    }

    private (bool Process, EFCachePolicy? CachePolicy) ShouldProcessExecutedCommands(DbCommand command,
        DbContext context)
    {
        if (!_cacheServiceCheck.IsCacheServiceAvailable())
        {
            return (false, null);
        }

        var (shouldSkipProcessing, cachePolicy) = _ignoreCachingProcessor.ShouldSkipProcessing(command, context);

        return shouldSkipProcessing ? (false, null) : (true, cachePolicy);
    }
}