using System;
using System.IO;

namespace VDS.Serialization
{
    /// <summary>
    /// Usability overloads for <see cref="ISerializer"/>.
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// Serializes the given data object as a string.
        /// </summary>
        public static string Serialize<T>(this ISerializer serializer, T data, ISerializationOptions options = null)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, data, options);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="System.InvalidCastException">The deserialized object is not of type <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(this ISerializer serializer, string serialized, ISerializationOptions options = null)
        {
            using (var reader = new StringReader(serialized))
            {
                return (T)serializer.Deserialize(reader, options);
            }
        }

        /// <summary>
        /// Deserializes the specified string into an object of <paramref name="targetType"/>
        /// </summary>
        public static object Deserialize(this ISerializer serializer, string serialized, Type targetType, ISerializationOptions options = null)
        {
            using (var reader = new StringReader(serialized))
            {
                return serializer.Deserialize(reader, targetType, options);
            }
        }
    }
}