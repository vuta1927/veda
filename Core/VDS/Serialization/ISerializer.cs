using System;
using System.IO;
using VDS.Dependency;

namespace VDS.Serialization
{
    /// <summary>
    /// Defines a serializer
    /// </summary>
    public interface ISerializer : ISingletonDependency
    {
        /// <summary>
        /// Serializes an object graph to a text reader.
        /// </summary>
        void Serialize(TextWriter writer, object graph, ISerializationOptions options = null);

        /// <summary>
        /// Deserializes an object graph from the specified text reader.
        /// </summary>
        object Deserialize(TextReader reader, ISerializationOptions options = null);

        /// <summary>
        /// Deserializes an object graph from the specified text reader.
        /// </summary>
        object Deserialize(TextReader reader, Type targetType, ISerializationOptions options = null);
    }
}
