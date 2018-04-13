using System;
using System.Collections;
using System.Diagnostics;
using VDS.Helpers.Extensions;
using JetBrains.Annotations;

namespace VDS.Helpers.Exception
{
    public static partial class Throw
    {
        #region Methods
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
        /// </summary>
        /// <param name="argument">The argument to check</param>
        /// <param name="argumentName">The name of the argument.</param>
        [DebuggerStepThrough]
        [ContractAnnotation("argument:null => halt")]
        public static void IfArgumentNull(object argument, [InvokerParameterName]string argumentName)
        {
            if (argument == null)
            {
                var ex = new ArgumentNullException(argumentName, "The argument '{0}' cannot be null.".FormatWith(argumentName));
                ThrowInternal(ex);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified argument is null or empty.
        /// </summary>
        /// <param name="argument">The argument to check</param>
        /// <param name="argumentName">The name of the argument.</param>
        [DebuggerStepThrough]
        [ContractAnnotation("argument:null => halt")]
        public static void IfArgumentNullOrEmpty(IEnumerable argument, [InvokerParameterName]string argumentName)
        {
            if (argument.IsNullOrEmpty())
            {
                var ex = new ArgumentException("The argument '{0}' cannot be null or empty.".FormatWith(argumentName), argumentName);
                ThrowInternal(ex);
            }
        }
        #endregion
    }
}
