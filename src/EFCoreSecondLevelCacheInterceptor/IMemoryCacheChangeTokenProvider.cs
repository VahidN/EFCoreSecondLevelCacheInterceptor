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
}