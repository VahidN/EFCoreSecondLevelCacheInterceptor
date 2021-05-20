namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// A hash provider contract
    /// </summary>
    public interface IEFHashProvider
    {
        /// <summary>
        /// Computes the Hash of the input string.
        /// </summary>
        /// <param name="data">the input string</param>
        /// <returns>Hash</returns>
        ulong ComputeHash(string data);

        /// <summary>
        /// Computes the hash of the input array.
        /// </summary>
        /// <param name="data">the input array</param>
        /// <returns>Hash</returns>
        ulong ComputeHash(byte[] data);

        /// <summary>
        /// Computes the hash of the input byte array.
        /// </summary>
        /// <param name="data">the input byte array</param>
        /// <param name="offset">start offset</param>
        /// <param name="len">length</param>
        /// <param name="seed">initial seed</param>
        /// <returns>Hash</returns>
        ulong ComputeHash(byte[] data, int offset, int len, uint seed);
    }
}