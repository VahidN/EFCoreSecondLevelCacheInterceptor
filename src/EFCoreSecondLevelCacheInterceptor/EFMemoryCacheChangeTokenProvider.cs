using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Propagates notifications that a change has occurred.
    /// </summary>
    public interface IMemoryCacheChangeTokenProvider
    {
        /// <summary>
        /// Gets or adds a change notification token.
        /// </summary>
        IChangeToken GetChangeToken(string key);

        /// <summary>
        /// Removes a change notification token.
        /// </summary>
        void RemoveChangeToken(string key);

        /// <summary>
        /// Removes all of the change notification tokens.
        /// </summary>
        void RemoveAllChangeTokens();
    }

    /// <summary>
    /// Propagates notifications that a change has occurred.
    /// </summary>
    public class EFMemoryCacheChangeTokenProvider : IMemoryCacheChangeTokenProvider
    {
        private readonly ConcurrentDictionary<string, ChangeTokenInfo> _changeTokens;

        /// <summary>
        /// Propagates notifications that a change has occurred.
        /// </summary>
        public EFMemoryCacheChangeTokenProvider()
        {
            _changeTokens = new ConcurrentDictionary<string, ChangeTokenInfo>();
        }

        /// <summary>
        /// Gets or adds a change notification token.
        /// </summary>
        public IChangeToken GetChangeToken(string key)
        {
            return _changeTokens.GetOrAdd(
                key,
                _ =>
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    var changeToken = new CancellationChangeToken(cancellationTokenSource.Token);
                    return new ChangeTokenInfo(changeToken, cancellationTokenSource);
                }).ChangeToken;
        }

        /// <summary>
        /// Removes a change notification token.
        /// </summary>
        public void RemoveChangeToken(string key)
        {
            if (_changeTokens.TryRemove(key, out ChangeTokenInfo changeTokenInfo))
            {
                changeTokenInfo.TokenSource.Cancel();
            }
        }

        /// <summary>
        /// Removes all of the change notification tokens.
        /// </summary>
        public void RemoveAllChangeTokens()
        {
            foreach (var item in _changeTokens)
            {
                RemoveChangeToken(item.Key);
            }
        }

        private struct ChangeTokenInfo
        {
            public ChangeTokenInfo(IChangeToken changeToken, CancellationTokenSource tokenSource)
            {
                ChangeToken = changeToken;
                TokenSource = tokenSource;
            }

            public IChangeToken ChangeToken { get; }

            public CancellationTokenSource TokenSource { get; }
        }
    }
}