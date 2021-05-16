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
        uint ComputeHash(string data);

        /// <summary>
        /// Computes the Hash of the input byte array.
        /// </summary>
        /// <param name="data">the input byte array</param>
        /// <returns>Hash</returns>
        uint ComputeHash(byte[] data);

        /// <summary>
        /// Computes the Hash of the input byte array.
        /// </summary>
        /// <param name="data">the input byte array</param>
        /// <param name="offset">start offset</param>
        /// <param name="len">length</param>
        /// <param name="seed">initial seed</param>
        /// <returns>Hash</returns>
        uint ComputeHash(byte[] data, int offset, uint len, uint seed);
    }
}