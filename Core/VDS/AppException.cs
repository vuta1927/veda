using System;
using System.Runtime.Serialization;

namespace VDS
{
    /// <summary>
    /// Base exception type for those are thrown by Application system for Application specific exceptions.
    /// </summary>
    [Serializable]
    public class AppException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        public AppException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        public AppException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public AppException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public AppException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}