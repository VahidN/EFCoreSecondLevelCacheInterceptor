using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Is the configured cache provider online?
/// </summary>
public class EFCacheServiceCheck : IEFCacheServiceCheck
{
    private readonly ILogger<EFCacheServiceCheck> _cacheServiceCheckLogger;
    private readonly IEFCacheServiceProvider _cacheServiceProvider;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
    private readonly IEFDebugLogger _logger;

    private bool? _isCacheServerAvailable;
    private DateTime? _lastCheckTime;

    /// <summary>
    ///     Is the configured cache provider online?
    /// </summary>
    public EFCacheServiceCheck(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
        IEFCacheServiceProvider cacheServiceProvider,
        IEFDebugLogger logger,
        ILogger<EFCacheServiceCheck> cacheServiceCheckLogger)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheServiceProvider = cacheServiceProvider;
        _logger = logger;
        _cacheServiceCheckLogger = cacheServiceCheckLogger;
        _cacheSettings = cacheSettings.Value;
    }

    /// <summary>
    ///     Is the configured cache services online and available? Can we use it without any problem?
    /// </summary>
    public bool IsCacheServiceAvailable()
    {
        if (!_cacheSettings.IsCachingInterceptorEnabled)
        {
            if (_logger.IsLoggerEnabled)
            {
                _cacheServiceCheckLogger.LogDebug(message: "The caching interceptor is disabled.");
            }

            return false;
        }

        if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
        {
            if (_logger.IsLoggerEnabled)
            {
                _cacheServiceCheckLogger.LogDebug(
                    message:
                    "The caching interceptor isn't set to fallback on db if the caching provider ({Type}) is down.",
                    _cacheServiceProvider.GetType());
            }

            return true;
        }

        if (_logger.IsLoggerEnabled)
        {
            _cacheServiceCheckLogger.LogDebug(
                message: "The caching interceptor is set to fallback on db if the caching provider ({Type}) is down.",
                _cacheServiceProvider.GetType());
        }

        var now = DateTime.UtcNow;

        if (_lastCheckTime.HasValue && _isCacheServerAvailable.HasValue &&
            now - _lastCheckTime.Value < _cacheSettings.NextCacheServerAvailabilityCheck)
        {
            return _isCacheServerAvailable.Value;
        }

        try
        {
            _lastCheckTime = now;

            _ = _cacheServiceProvider.GetValue(new EFCacheKey(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "__Name__"
            })
            {
                KeyHash = "__Test__"
            }, new EFCachePolicy());

            _isCacheServerAvailable = true;

            if (_logger.IsLoggerEnabled)
            {
                _cacheServiceCheckLogger.LogDebug(message: "The cache service is available.");
            }
        }
        catch (Exception ex)
        {
            _isCacheServerAvailable = false;

            if (_logger.IsLoggerEnabled)
            {
                _cacheServiceCheckLogger.LogDebug(ex, message: "The cache service({Type}) isn't available.",
                    _cacheServiceProvider.GetType());
            }
        }

        return _isCacheServerAvailable.Value;
    }
}