using System;

namespace VDS.Serialization
{
    /// <summary>
    /// Represents the flag options for serialization
    /// </summary>
    [Flags]
    public enum SerializationOptionsFlags
    {
        /// <summary>
        /// No specific options
        /// </summary>
        None = 0,

        /// <summary>
        /// Use camel case for naming of properties
        /// </summary>
        UseCamelCase = 1,

        /// <summary>
        /// Include type names during serialization
        /// </summary>
        IncludeTypeNames = 2,
    }
}