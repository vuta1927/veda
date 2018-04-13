using System;
using VDS.Helpers.Exception;

namespace VDS.Validation
{
    /// <summary>
    /// Represents a model validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Initialies a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        public ValidationError(string name, object attemptedValue, string message)
        {
            Name = name ?? "";
            Message = message ?? "";
            AttemptedValue = attemptedValue;
        }

        /// <summary>
        /// Initialies a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attemptedValue"></param>
        /// <param name="exception"></param>
        public ValidationError(string name, object attemptedValue, Exception exception)
        {
            Throw.IfArgumentNull(exception, nameof(exception));

            Name = name ?? "";
            AttemptedValue = attemptedValue;
            Exception = exception;
            Message = exception.Message;
        }

        /// <summary>
        /// Gets the member name that a part of the error.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the member value.
        /// </summary>
        public object AttemptedValue { get; private set; }

        /// <summary>
        /// Gets the errors message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the error exception.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}