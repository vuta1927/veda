using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace VDS.Helpers.Exception
{
    /// <summary>
    /// Provides static methods for conditional exceptions.
    /// </summary>
    public static partial class Throw
    {
        #region Methods
        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <param name="exception">The exception to be thrown.</param>
        /// <param name="modifier">[Optional] A modifier used to modify the exception before throwing.</param>
        [DebuggerStepThrough]
        internal static void ThrowInternal(System.Exception exception, Func<System.Exception, System.Exception> modifier = null)
        {
            ThrowInternal<System.Exception>(exception, modifier);
        }

        /// <summary>
        /// Throws the specified exception.
        /// </summary>
        /// <param name="exception">The exception to be thrown.</param>
        /// <param name="modifier">[Optional] A modifier used to modify the exception before throwing.</param>
        [DebuggerStepThrough]
        internal static void ThrowInternal<TException>(TException exception, Func<TException, System.Exception> modifier = null)
            where TException : System.Exception
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            System.Exception modifiedException = exception;

            if (modifier != null)
            {
                modifiedException = modifier(exception);
            }

            throw modifiedException ?? exception;
        }

        /// <summary>
        /// Throws an exception immediately.
        /// </summary>
        /// <returns>An instance of <see cref="ThrowEvaluation"/> used to throw exceptions.</returns>
        public static ThrowEvaluation Now
        {
            [DebuggerStepThrough]
            [ContractAnnotation("=> halt")]  // Unfortunately this attribute is ignored.
            get { return new ThrowEvaluation(true); }
        }
        #endregion
    }
}