using System.Collections.Generic;

namespace VDS.Notifications
{
    /// <summary>
    /// Used to store data for a notification
    /// It can be directly used or can be derived.
    /// </summary>
    public interface INotificationData
    {
        /// <summary>
        /// Gets notification data type name.
        /// It returns the full class name by default.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Shortcut to set/get <see cref="Properties"/>.
        /// </summary>
        object this[string key] { get; set; }

        /// <summary>
        /// Can be used to add custom properties to this notification.
        /// </summary>
        Dictionary<string, object> Properties { get; set; }
    }
}