using System;
using System.Runtime.Serialization;

namespace VDS.Data.Uow
{
    [Serializable]
    public class DataConcurrencyException : AppException
    {
        /// <summary>
        /// Creates a new <see cref="DataConcurrencyException"/> object.
        /// </summary>
        public DataConcurrencyException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="AppException"/> object.
        /// </summary>
        public DataConcurrencyException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DataConcurrencyException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public DataConcurrencyException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="DataConcurrencyException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public DataConcurrencyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}