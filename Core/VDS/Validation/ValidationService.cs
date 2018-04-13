using System.Collections.Generic;
using System.Linq;
using VDS.Configuration;
using VDS.Helpers.Exception;

namespace VDS.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IEnumerable<IValidator> _validators;
        private readonly IValidationConfiguration _validationConfiguration;

        public ValidationService(IEnumerable<IValidator> validators, IValidationConfiguration validationConfiguration)
        {
            _validators = validators;
            _validationConfiguration = validationConfiguration;
        }

        public ValidationResult ValidateFor(object instance)
        {
            Throw.IfArgumentNull(instance, nameof(instance));

            if (_validationConfiguration.IgnoredTypes.Any(t => t.IsInstanceOfType(instance)))
                return new ValidationResult();

            var errors = new ValidationErrorCollection();
            foreach (var validator in _validators)
            {
                foreach (var validationError in validator.Validate(instance))
                {
                    errors.Add(validationError);
                }
            }
            var result = new ValidationResult();
            result.Errors.AddRange(errors);
            return result;
        }
    }
}