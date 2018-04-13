using System;

namespace VDS.Serialization
{
    /// <summary>
    /// Represents the options for serialization
    /// </summary>
    public class SerializationOptions : ISerializationOptions
    {
        private static readonly SerializationOptions DefaultOptions = new SerializationOptions(SerializationOptionsFlags.None);
        private static readonly SerializationOptions CamelCaseOptions = new SerializationOptions(SerializationOptionsFlags.UseCamelCase);
        private static readonly SerializationOptions IncludeTypeNamesOptions = new SerializationOptions(SerializationOptionsFlags.IncludeTypeNames);

        /// <summary>
        /// Gets the default serialization options that will serialize all properties without any special flags.
        /// </summary>
        public static ISerializationOptions Default => DefaultOptions;

        /// <summary>
        /// Gets the camel case serialization options that will serialize all properties using camel case.
        /// </summary>
        public static ISerializationOptions CamelCase => CamelCaseOptions;

        /// <summary>
        /// Gets the type names serialization options that will serialize all properties including type names.
        /// </summary>
        public static ISerializationOptions IncludeTypeNames => IncludeTypeNamesOptions;

        /// <summary>
        /// Initializes a new instance of <see cref="SerializationOptions"/>
        /// </summary>
        /// <param name="flags">The serialization flags</param>
        /// <remarks>
        /// All instances of this class or subclasses must be immutable, because mapping from
        /// serialization options to contract resolvers are cached for performance reasons.
        /// </remarks>
        protected SerializationOptions(SerializationOptionsFlags flags)
        {
            Flags = flags;
        }

        /// <inheritdoc />
        /// <summary>
        /// Will always return true
        /// </summary>
        public virtual bool ShouldSerializeProperty(Type type, string propertyName)
        {
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the serialization flags
        /// </summary>
        public SerializationOptionsFlags Flags { get; private set; }
    }
}