using System;

namespace VDS.Settings
{
    /// <summary>
    /// Defines an exception to be thrown when a problem occurs in setting registration.
    /// </summary>
    public class SettingRegistrationException : Exception
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="SettingRegistrationException"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public SettingRegistrationException(string message)
            : base(message)
        {
        }
        #endregion
    }
}