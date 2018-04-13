using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.Helpers.Extensions;
using JetBrains.Annotations;

namespace VDS.Helpers.Exception
{
    /// <summary>
    /// Throws evaluated conditions.
    /// </summary>
    public class ThrowEvaluation
    {
        private readonly bool _throwException;

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="ThrowEvaluation"/>.
        /// </summary>
        /// <param name="throwException">Should an exception be thrown?</param>
        internal ThrowEvaluation(bool throwException)
        {
            _throwException = throwException;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Throws an exception of the specified type.
        /// </summary>
        /// <typeparam name="TException">The type of exception to be thrown.</typeparam>
        /// <param name="modifier">A delegate used to modifier the exception before being thrown.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="args">An object array of other parameters <see cref="TException"/> type constructor accepts.</param>
        [DebuggerStepThrough]
        public void A<TException>(Func<TException, System.Exception> modifier, string message, System.Exception innerException, params object[] args)
            where TException : System.Exception
        {
            if (_throwException)
            {
                var parameters = new List<object>((new object[] { message, innerException }).Concat(args));
                var exceptionActivator = ObjectFactory.GetActivator<TException>(parameters.Select(p => p == null ? typeof(object) : p.GetType()).ToArray());
                if (exceptionActivator == null)
                    throw new ThrowingException(typeof(TException), message,
                            "An exception of type '{0}' could not be thrown because that exception type does not define a public constructor that takes a single string argument.  The original message was: {1}."
                            .FormatWith(typeof(TException).FullName, message));

                Throw.ThrowInternal(exceptionActivator(parameters.ToArray()), modifier);
            }
        }

        /// <summary>
        /// Throws an exception of the specified type.
        /// </summary>
        /// <typeparam name="TException">The type of exception to be thrown.</typeparam>
        /// <param name="modifier">A delegate used to modifier the exception before being thrown.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="args">An object array of other parameters <see cref="TException"/> type constructor accepts.</param>
        [DebuggerStepThrough]
        public void A<TException>(Func<TException, System.Exception> modifier, string message, params object[] args)
            where TException : System.Exception
        {
            if (_throwException)
            {
                var parameters = new List<object>((new object[] { message }).Concat(args));
                var exceptionActivator = ObjectFactory.GetActivator<TException>(parameters.Select(p => p == null ? typeof(object) : p.GetType()).ToArray());
                if (exceptionActivator == null)
                    throw new ThrowingException(typeof(TException), message,
                            "An exception of type '{0}' could not be thrown because that exception type does not define a public constructor that takes a single string argument.  The original message was: {1}."
                            .FormatWith(typeof(TException).FullName, message));

                Throw.ThrowInternal(exceptionActivator(parameters.ToArray()), modifier);
            }
        }

        /// <summary>
        /// Throws an exception of the specified type.
        /// </summary>
        /// <typeparam name="TException">The type of exception to be thrown.</typeparam>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="args">An object array of other parameters <see cref="TException"/> type constructor accepts.</param>
        [DebuggerStepThrough]
        public void A<TException>(string message, System.Exception innerException, params object[] args)
            where TException : System.Exception
        {
            A((Func<TException, System.Exception>)null, message, innerException, args);
        }

        /// <summary>
        /// Throws an exception of the specified type.
        /// </summary>
        /// <typeparam name="TException">The type of exception to be thrown.</typeparam>
        /// <param name="message">The exception message.</param>
        /// <param name="args">An object array of other parameters <see cref="TException"/> type constructor accepts.</param>
        [DebuggerStepThrough]
        public void A<TException>(string message, params object[] args)
            where TException : System.Exception
        {
            A((Func<TException, System.Exception>)null, message, args);
        }

        /// <summary>
        /// Throws an exception of the specified type.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="message">The exception message.</param>
        [DebuggerStepThrough]
        public void AnArgumentException(string message, [InvokerParameterName]string argumentName = null)
        {
            if (_throwException)
            {
                var ex = new ArgumentException(message, argumentName);
                Throw.ThrowInternal(ex);
            }
        }
        #endregion
    }
}