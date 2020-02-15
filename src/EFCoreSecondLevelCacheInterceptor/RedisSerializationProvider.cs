using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    /// Serialization Provider
    /// </summary>
    public interface IRedisSerializationProvider
    {
        /// <summary>
        /// Deserialize the given byte array to an object.
        /// </summary>
        object Deserialize(byte[] value);

        /// <summary>
        /// Deserialize the given byte array to an object.
        /// </summary>
        T Deserialize<T>(byte[] value);

        /// <summary>
        /// Serializes the given data to a byte array.
        /// </summary>
        byte[] Serialize(object value);
    }

    /// <summary>
    /// Serialization Provider
    /// </summary>
    public class RedisSerializationProvider : IRedisSerializationProvider
    {
        /// <summary>
        /// Serializes the given data to a byte array.
        /// </summary>
        public byte[] Serialize(object value)
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the given byte array to an object.
        /// </summary>
        public object Deserialize(byte[] value)
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream(value))
            {
                return serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserialize the given byte array to an object.
        /// </summary>
        public T Deserialize<T>(byte[] value)
        {
            return (T)Convert.ChangeType(Deserialize(value), typeof(T));
        }
    }
}