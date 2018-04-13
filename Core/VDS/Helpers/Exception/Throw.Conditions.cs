using System.Collections;
using System.Diagnostics;
using VDS.Helpers.Extensions;
using JetBrains.Annotations;

namespace VDS.Helpers.Exception
{
    //Bug [01271734]: Checking an object for null and then using that object in ThrowEvaluation methods results in an NullReferenceException
    static partial class Throw
    {
        #region Methods
        /// <summary>
        /// Evaluates whether an exception can be thrown.
        /// </summary>
        /// <param name="condition">The condition to throw exception.</param>
        /// <returns>An instance of <see cref="ThrowEvaluation"/> used to throw exceptions.</returns>
        [DebuggerStepThrough]
        [ContractAnnotation("true => halt")]
        public static ThrowEvaluation If(bool condition)
        {
            return new ThrowEvaluation(condition);
        }

        /// <summary>
        /// Evaluates whether an exception can be thrown.
        /// </summary>
        /// <param name="condition">The condition to throw exception.</param>
        /// <returns>An instance of <see cref="ThrowEvaluation"/> used to throw exceptions.</returns>
        [DebuggerStepThrough]
        [ContractAnnotation("false => halt")]
        public static ThrowEvaluation IfNot(bool condition)
        {
            return new ThrowEvaluation(!condition);
        }

        /// <summary>
        /// Evaluates whether an exception can be thrown.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>An instance of <see cref="ThrowEvaluation"/> used to throw exceptions.</returns>
        [DebuggerStepThrough]
        [ContractAnnotation("null => halt")]
        public static ThrowEvaluation IfNull(object value)
        {
            return new ThrowEvaluation(value == null);
        }

        /// <summary>
        /// Evaluates whether an exception can be thrown.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>An instance of <see cref="ThrowEvaluation"/> used to throw exceptions.</returns>
        [DebuggerStepThrough]
        [ContractAnnotation("null => halt")]
        public static ThrowEvaluation IfNullOrEmpty(IEnumerable value)
        {
            return new ThrowEvaluation(value.IsNullOrEmpty());
        }
        #endregion
    }
}