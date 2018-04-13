using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VDS.Validation
{
    /// <summary>
    /// Represents the collection results of a model validation.
    /// </summary>
    public class ValidationErrorCollection : Collection<ValidationError>
    {
        public void Add(string name, object attemptedValue, string message)
        {
            Add(new ValidationError(name, attemptedValue, message));
        }

        public void Add(string name, object attemptedValue, Exception exception)
        {
            Add(new ValidationError(name, attemptedValue, exception));
        }

        public void AddRange(IEnumerable<ValidationError> errors)
        {
            foreach (var validationError in errors)
            {
                Add(validationError);
            }
        }
    }
}