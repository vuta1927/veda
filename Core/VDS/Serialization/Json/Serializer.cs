using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace VDS.Serialization.Json
{
    /// <summary>
    /// Represents a <see cref="ISerializer"/>
    /// </summary>
    public class Serializer : ISerializer
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<ISerializationOptions, JsonSerializer> _cacheAutoTypeName;
        private readonly ConcurrentDictionary<ISerializationOptions, JsonSerializer> _cacheNoneTypeName;

        /// <summary>
        /// Initializes a new instance of <see cref="Serializer"/>
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> used to create instances of types during serialization</param>
        public Serializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _cacheAutoTypeName = new ConcurrentDictionary<ISerializationOptions, JsonSerializer>();
            _cacheNoneTypeName = new ConcurrentDictionary<ISerializationOptions, JsonSerializer>();
        }
        
        public void Serialize(TextWriter writer, object graph, ISerializationOptions options = null)
        {
            var jsonWriter = new JsonTextWriter(writer);
            var serializer = CreateSerializerForSerialization(options);

            serializer.Serialize(jsonWriter, graph);

            // We don't close the stream as it's owned by the message.
            writer.Flush();
        }

        public object Deserialize(TextReader reader, ISerializationOptions options = null)
        {
            var jsonReader = new JsonTextReader(reader);

            try
            {
                var serializer = CreateSerializerForDeserialization(options);
                return serializer.Deserialize(jsonReader);
            }
            catch (JsonSerializationException e)
            {
                // Wrap in a standard .NET exception.
                throw new SerializationException(e.Message, e);
            }
        }

        public object Deserialize(TextReader reader, Type targetType, ISerializationOptions options = null)
        {
            var jsonReader = new JsonTextReader(reader);

            try
            {
                var serializer = CreateSerializerForDeserialization(options);
                return serializer.Deserialize(jsonReader, targetType);
            }
            catch (JsonSerializationException e)
            {
                throw new SerializationException(e.Message, e);
            }
        }

        private JsonSerializer CreateSerializerForDeserialization(ISerializationOptions options)
        {
            return RetrieveSerializer(options ?? SerializationOptions.Default, true);
        }

        private JsonSerializer CreateSerializerForSerialization(ISerializationOptions options)
        {
            if (options == null)
            {
                options = SerializationOptions.Default;
            }

            return RetrieveSerializer(options, options.Flags.HasFlag(SerializationOptionsFlags.IncludeTypeNames));
        }

        private JsonSerializer RetrieveSerializer(ISerializationOptions options, bool includeTypeNames)
        {
            if (includeTypeNames)
            {
                return _cacheAutoTypeName.GetOrAdd(options, _ => CreateSerializer(options, TypeNameHandling.Auto));
            }
            return _cacheNoneTypeName.GetOrAdd(options, _ => CreateSerializer(options, TypeNameHandling.None));
        }

        private JsonSerializer CreateSerializer(ISerializationOptions options, TypeNameHandling typeNameHandling)
        {
            var contractResolver = new SerializerContractResolver(_serviceProvider, options);

            var serializer = new JsonSerializer
            {
                TypeNameHandling = typeNameHandling,
                ContractResolver = contractResolver,
            };

            serializer.Converters.Add(new ExceptionConverter());

            return serializer;
        }
    }
}