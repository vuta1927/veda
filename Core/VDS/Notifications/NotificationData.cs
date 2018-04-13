using System;
using System.Collections.Generic;
using VDS.Helpers.Extensions;
using VDS.Json;

namespace VDS.Notifications
{
    /// <inheritdoc />
    [Serializable]
    public class NotificationData : INotificationData
    {
        /// <inheritdoc />
        public virtual string Type => GetType().FullName;
        
        /// <inheritdoc />
        public object this[string key]
        {
            get => Properties.GetOrDefault(key);
            set => Properties[key] = value;
        }

        /// <inheritdoc />
        public Dictionary<string, object> Properties
        {
            get => _properties;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                /* Not assign value, but add dictionary items. This is required for backward compability. */
                foreach (var keyValue in value)
                {
                    if (!_properties.ContainsKey(keyValue.Key))
                    {
                        _properties[keyValue.Key] = keyValue.Value;
                    }
                }
            }
        }
        private readonly Dictionary<string, object> _properties;

        /// <summary>
        /// Createa a new NotificationData object.
        /// </summary>
        public NotificationData()
        {
            _properties = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return this.ToJsonString();
        }
    }
}