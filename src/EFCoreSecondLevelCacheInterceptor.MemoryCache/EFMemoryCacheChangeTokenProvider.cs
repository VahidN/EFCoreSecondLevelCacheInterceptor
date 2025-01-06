using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Propagates notifications that a change has occurred.
/// </summary>
public class EFMemoryCacheChangeTokenProvider : IMemoryCacheChangeTokenProvider
{
    private readonly ConcurrentDictionary<string, ChangeTokenInfo> _changeTokens;

    /// <summary>
    ///     Propagates notifications that a change has occurred.
    /// </summary>
    public EFMemoryCacheChangeTokenProvider()
        => _changeTokens = new ConcurrentDictionary<string, ChangeTokenInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets or adds a change notification token.
    /// </summary>
    public IChangeToken GetChangeToken(string key)
        => _changeTokens.GetOrAdd(key, _ =>
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var changeToken = new CancellationChangeToken(cancellationTokenSource.Token);

                return new ChangeTokenInfo(changeToken, cancellationTokenSource);
            })
            .ChangeToken;

    /// <summary>
    ///     Removes a change notification token.
    /// </summary>
    public void RemoveChangeToken(string key)
    {
        if (_changeTokens.TryRemove(key, out var changeTokenInfo))
        {
            changeTokenInfo.TokenSource.Cancel();
        }
    }

    /// <summary>
    ///     Removes all the change notification tokens.
    /// </summary>
    public void RemoveAllChangeTokens()
    {
        foreach (var item in _changeTokens)
        {
            RemoveChangeToken(item.Key);
        }
    }

    private readonly struct ChangeTokenInfo(IChangeToken changeToken, CancellationTokenSource tokenSource)
    {
        public IChangeToken ChangeToken { get; } = changeToken;

        public CancellationTokenSource TokenSource { get; } = tokenSource;
    }
}