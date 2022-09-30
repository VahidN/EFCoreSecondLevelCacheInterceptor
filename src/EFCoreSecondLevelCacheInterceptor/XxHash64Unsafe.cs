using System;
using System.Runtime.CompilerServices;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     xxHash is an extremely fast non-cryptographic Hash algorithm, working at speeds close to RAM limits.
///     https://github.com/Cyan4973/xxHash
/// </summary>
public class XxHash64Unsafe : IEFHashProvider
{
    private const ulong Prime1 = 11400714785074694791UL;
    private const ulong Prime2 = 14029467366897019727UL;
    private const ulong Prime3 = 1609587929392839161UL;
    private const ulong Prime4 = 9650029242287828579UL;
    private const ulong Prime5 = 2870177450012600261UL;
    private const ulong Seed = 0UL;

    /// <summary>
    ///     Computes the xxHash64 of the input string. xxHash64 is an extremely fast non-cryptographic Hash algorithm.
    /// </summary>
    /// <param name="data">the input string</param>
    /// <returns>xxHash64</returns>
    public unsafe ulong ComputeHash(string data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        fixed (char* input = data)
        {
            return ComputeHash((byte*)input, data.Length * sizeof(char), Seed);
        }
    }

    /// <summary>
    ///     Computes the xxHash64 of the input array. xxHash is an extremely fast non-cryptographic Hash algorithm.
    /// </summary>
    /// <param name="data">the input array</param>
    /// <returns>xxHash64</returns>
    public unsafe ulong ComputeHash(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        fixed (byte* input = &data[0])
        {
            return ComputeHash(input, data.Length, Seed);
        }
    }

    /// <summary>
    ///     Computes the xxHash64 of the input byte array. xxHash is an extremely fast non-cryptographic Hash algorithm.
    /// </summary>
    /// <param name="data">the input byte array</param>
    /// <param name="offset">start offset</param>
    /// <param name="len">length</param>
    /// <param name="seed">initial seed</param>
    /// <returns>xxHash64</returns>
    public unsafe ulong ComputeHash(byte[] data, int offset, int len, uint seed)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        fixed (byte* input = &data[offset])
        {
            return ComputeHash(input, len, seed);
        }
    }

    /// <summary>
    ///     Computes the xxHash64 of the input string. xxHash is an extremely fast non-cryptographic Hash algorithm.
    /// </summary>
    /// <param name="ptr"></param>
    /// <param name="length"></param>
    /// <param name="seed"></param>
    /// <returns>xxHash</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong ComputeHash(byte* ptr, int length, ulong seed)
    {
        var end = ptr + length;
        ulong h64;

        if (length >= 32)
        {
            var limit = end - 32;

            var v1 = seed + Prime1 + Prime2;
            var v2 = seed + Prime2;
            var v3 = seed + 0;
            var v4 = seed - Prime1;

            do
            {
                v1 += *(ulong*)ptr * Prime2;
                v1 = RotateLeft(v1, 31); // rotl 31
                v1 *= Prime1;
                ptr += 8;

                v2 += *(ulong*)ptr * Prime2;
                v2 = RotateLeft(v2, 31); // rotl 31
                v2 *= Prime1;
                ptr += 8;

                v3 += *(ulong*)ptr * Prime2;
                v3 = RotateLeft(v3, 31); // rotl 31
                v3 *= Prime1;
                ptr += 8;

                v4 += *(ulong*)ptr * Prime2;
                v4 = RotateLeft(v4, 31); // rotl 31
                v4 *= Prime1;
                ptr += 8;
            } while (ptr <= limit);

            h64 = RotateLeft(v1, 1) + // rotl 1
                  RotateLeft(v2, 7) + // rotl 7
                  RotateLeft(v3, 12) + // rotl 12
                  RotateLeft(v4, 18); // rotl 18

            // merge round
            v1 *= Prime2;
            v1 = RotateLeft(v1, 31); // rotl 31
            v1 *= Prime1;
            h64 ^= v1;
            h64 = h64 * Prime1 + Prime4;

            // merge round
            v2 *= Prime2;
            v2 = RotateLeft(v2, 31); // rotl 31
            v2 *= Prime1;
            h64 ^= v2;
            h64 = h64 * Prime1 + Prime4;

            // merge round
            v3 *= Prime2;
            v3 = RotateLeft(v3, 31); // rotl 31
            v3 *= Prime1;
            h64 ^= v3;
            h64 = h64 * Prime1 + Prime4;

            // merge round
            v4 *= Prime2;
            v4 = RotateLeft(v4, 31); // rotl 31
            v4 *= Prime1;
            h64 ^= v4;
            h64 = h64 * Prime1 + Prime4;
        }
        else
        {
            h64 = seed + Prime5;
        }

        h64 += (ulong)length;

        // finalize
        while (ptr <= end - 8)
        {
            var t1 = *(ulong*)ptr * Prime2;
            t1 = RotateLeft(t1, 31); // rotl 31
            t1 *= Prime1;
            h64 ^= t1;
            h64 = RotateLeft(h64, 27) * Prime1 + Prime4; // (rotl 27) * p1 + p4
            ptr += 8;
        }

        if (ptr <= end - 4)
        {
            h64 ^= *(uint*)ptr * Prime1;
            h64 = RotateLeft(h64, 23) * Prime2 + Prime3; // (rotl 23) * p2 + p3
            ptr += 4;
        }

        while (ptr < end)
        {
            h64 ^= *ptr * Prime5;
            h64 = RotateLeft(h64, 11) * Prime1; // (rotl 11) * p1
            ptr += 1;
        }

        // avalanche
        h64 ^= h64 >> 33;
        h64 *= Prime2;
        h64 ^= h64 >> 29;
        h64 *= Prime3;
        h64 ^= h64 >> 32;

        return h64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateLeft(ulong value, int offset)
    {
#if NET5_0 || NETCORE3_1
            return System.Numerics.BitOperations.RotateLeft(value, offset);
#else
        return (value << offset) | (value >> (64 - offset));
#endif
    }
}