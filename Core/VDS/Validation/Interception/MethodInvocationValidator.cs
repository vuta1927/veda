using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Dependency;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using VDS.Helpers.Reflection;
using VDS.Reflection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Validation.Interception
{
    /// <summary>
    /// This class is used to validate a method call (invocation) for method arguments.
    /// </summary>
    public class MethodInvocationValidator : ITransientDependency
    {
        protected MethodInfo Method { get; private set; }
        protected object[] ParameterValues { get; private set; }
        protected ParameterInfo[] Parameters { get; private set; }
        protected List<ValidationResult> ValidationResults { get; }

        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new <see cref="MethodInvocationValidator"/> instance.
        /// </summary>
        public MethodInvocationValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            ValidationResults = new List<ValidationResult>();
        }

        /// <param name="method">Method to be validated</param>
        /// <param name="parameterValues">List of arguments those are used to call the <paramref name="method"/>.</param>
        public virtual void Initialize(MethodInfo method, object[] parameterValues)
        {
            Throw.IfArgumentNull(method, nameof(method));
            Throw.IfArgumentNull(parameterValues, nameof(parameterValues));

            Method = method;
            ParameterValues = parameterValues;
            Parameters = method.GetParameters();
        }

        /// <summary>
        /// Validates the method invocation.
        /// </summary>
        public void Validate()
        {
            CheckInitialized();

            if (Parameters.IsNullOrEmpty())
            {
                return;
            }

            if (!Method.IsPublic)
            {
                return;
            }

            if (IsValidationDisabled())
            {
                return;
            }

            if (Parameters.Length != ParameterValues.Length)
            {
                throw new Exception("Method parameter count does not match with argument count!");
            }

            if (ValidationResults.Any(vr => !vr.IsValid) && HasSingleNullArgument())
            {
                ThrowValidationError();
            }

            for (var i = 0; i < Parameters.Length; i++)
            {
                ValidateMethodParameter(Parameters[i], ParameterValues[i]);
            }

            if (ValidationResults.Any(vr => !vr.IsValid))
            {
                ThrowValidationError();
            }
        }

        protected virtual void CheckInitialized()
        {
            if (Method == null)
            {
                throw new AppException("This object has not been initialized. Call Initialize method first.");
            }
        }

        protected virtual bool IsValidationDisabled()
        {
            if (Method.IsDefined(typeof(EnableValidationAttribute), true))
            {
                return false;
            }

            return ReflectionUtil.GetSingleAttributeOfMemberOrDeclaringTypeOrDefault<DisableValidationAttribute>(Method) != null;
        }

        protected virtual bool HasSingleNullArgument()
        {
            return Parameters.Length == 1 && ParameterValues[0] == null;
        }

        protected virtual void ThrowValidationError()
        {
            throw new ValidationException(
                "Method arguments are not valid! See ValidationErrors for details.",
                ValidationResults
            );
        }

        /// <summary>
        /// Validates given parameter for given value.
        /// </summary>
        /// <param name="parameterInfo">Parameter of the method to validate</param>
        /// <param name="parameterValue">Value to validate</param>
        protected virtual void ValidateMethodParameter(ParameterInfo parameterInfo, object parameterValue)
        {
            if (parameterValue == null)
            {
                if (!parameterInfo.IsOptional &&
                    !parameterInfo.IsOut &&
                    !parameterInfo.ParameterType.IsPrimitiveExtendedIncludingNullable(includeEnums: true))
                {
                    var validationResult = new ValidationResult();
                    validationResult.Errors.Add(parameterInfo.Name, null, parameterInfo.Name + "is null!");
                    ValidationResults.Add(validationResult);
                }

                return;
            }
            var result = _serviceProvider.GetService<IValidationService>().ValidateFor(parameterValue);
            if (!result.IsValid)
                ValidationResults.Add(result);
        }
    }
}