using System.Collections.Generic;

namespace VDS.Validation
{
    /// <summary>
    /// Provides a way to validate a type as well as a description to use for client-side validation
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validates the specified instance.
        /// </summary>
        /// <param name="instance">The instance that should be validated.</param>
        /// <returns>The validate result</returns>
        IEnumerable<ValidationError> Validate(object instance);
    }
}