using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Helps process SecondLevelCacheInterceptor
/// </summary>
public class DbCommandIgnoreCachingProcessor(
    IEFCachePolicyParser cachePolicyParser,
    IEFSqlCommandsProcessor sqlCommandsProcessor,
    IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
    IEFDebugLogger logger,
    ILogger<DbCommandIgnoreCachingProcessor> interceptorProcessorLogger) : IDbCommandIgnoreCachingProcessor
{
    /// <summary>
    ///     Is this command marked for caching?
    /// </summary>
    public (bool ShouldSkipProcessing, EFCachePolicy? CachePolicy) ShouldSkipProcessing(DbCommand? command,
        DbContext? context,
        CancellationToken cancellationToken = default)
    {
        if (context is null || command is null)
        {
            return (true, null);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return (true, null);
        }

        var commandCommandText = command.CommandText ?? "";

        if (ShouldSkipCachingDbContext(context, commandCommandText))
        {
            return (true, null);
        }

        var (cachePolicy, shouldSkipCaching) = GetCachePolicy(context, commandCommandText);

        if (shouldSkipCaching)
        {
            return (true, cachePolicy);
        }

        if (ShouldSkipQueriesInsideExplicitTransaction(command))
        {
            return (!sqlCommandsProcessor.IsCrudCommand(commandCommandText), cachePolicy);
        }

        if (sqlCommandsProcessor.IsCrudCommand(commandCommandText))
        {
            return (false, cachePolicy);
        }

        return cachePolicy is null ? (true, null) : (false, cachePolicy);
    }

    /// <summary>
    ///     Skip caching of this result based on the provided predicate
    /// </summary>
    public bool ShouldSkipCachingResults(string commandText, object value)
    {
        var result = cacheSettings.Value.SkipCachingResults != null &&
                     cacheSettings.Value.SkipCachingResults((commandText, value));

        if (result && logger.IsLoggerEnabled)
        {
            var message = "Skipped caching of this result based on the provided predicate.";
            interceptorProcessorLogger.LogDebug(message);
            logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText, efCacheKey: null);
        }

        return result;
    }

    private bool ShouldSkipCachingDbContext(DbContext context, string commandText)
    {
        var result = cacheSettings.Value.SkipCachingDbContexts is not null &&
                     cacheSettings.Value.SkipCachingDbContexts.Contains(context.GetType());

        if (result && logger.IsLoggerEnabled)
        {
            var message = $"Skipped caching of this DbContext: {context.GetType()}";
            interceptorProcessorLogger.LogDebug(message);
            logger.NotifyCacheableEvent(CacheableLogEventId.CachingSkipped, message, commandText, efCacheKey: null);
        }

        return result;
    }

    private bool ShouldSkipQueriesInsideExplicitTransaction(DbCommand? command)
        => !cacheSettings.Value.AllowCachingWithExplicitTransactions && command?.Transaction is not null;

    private (EFCachePolicy? CachePolicy, bool ShouldSkipCaching) GetCachePolicy(DbContext context, string commandText)
    {
        var allEntityTypes = sqlCommandsProcessor.GetAllTableNames(context);

        return cachePolicyParser.GetEFCachePolicy(commandText, allEntityTypes);
    }
}