using System;

namespace VDS.Notifications
{
    /// <summary>
    /// Arguments for <see cref="NotificationDistributerJob"/>.
    /// </summary>
    [Serializable]
    public class NotificationDistributerJobArgs
    {
        /// <summary>
        /// Notification Id.
        /// </summary>
        public Guid NotificationId { get; set; }

        public NotificationDistributerJobArgs(Guid notificationId)
        {
            NotificationId = notificationId;
        }
    }
}