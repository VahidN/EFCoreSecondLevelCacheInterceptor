using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Is the configured cache provider online?
/// </summary>
public class EFCacheServiceCheck : IEFCacheServiceCheck
{
    private readonly IEFCacheServiceProvider _cacheServiceProvider;
    private readonly EFCoreSecondLevelCacheSettings _cacheSettings;

    private bool? _isCacheServerAvailable;
    private DateTime? _lastCheckTime;

    /// <summary>
    ///     Is the configured cache provider online?
    /// </summary>
    public EFCacheServiceCheck(IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
                               IEFCacheServiceProvider cacheServiceProvider)
    {
        if (cacheSettings == null)
        {
            throw new ArgumentNullException(nameof(cacheSettings));
        }

        _cacheServiceProvider = cacheServiceProvider;
        _cacheSettings = cacheSettings.Value;
    }

    /// <summary>
    ///     Is the configured cache services online and available? Can we use it without any problem?
    /// </summary>
    public bool IsCacheServiceAvailable()
    {
        if (!_cacheSettings.UseDbCallsIfCachingProviderIsDown)
        {
            return true;
        }

        var now = DateTime.UtcNow;

        if (_lastCheckTime.HasValue &&
            _isCacheServerAvailable.HasValue &&
            now - _lastCheckTime.Value < _cacheSettings.NextCacheServerAvailabilityCheck)
        {
            return _isCacheServerAvailable.Value;
        }

        try
        {
            _lastCheckTime = now;
            _ = _cacheServiceProvider.GetValue(new EFCacheKey(new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                                              { "__Name__" }) { KeyHash = "__Test__" },
                                               new EFCachePolicy());
            _isCacheServerAvailable = true;
        }
        catch
        {
            _isCacheServerAvailable = false;
            if (_cacheSettings.UseDbCallsIfCachingProviderIsDown)
            {
                throw;
            }
        }

        return _isCacheServerAvailable.Value;
    }
}