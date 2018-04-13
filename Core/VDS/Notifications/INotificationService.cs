using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.Domain.Entities;

namespace VDS.Notifications
{
    public interface INotificationService
    {
        /// <summary>
        /// Send a new notification.
        /// </summary>
        /// <param name="notificationName">Unique notification name</param>
        /// <param name="data">Notification data (optional).</param>
        /// <param name="entityIdentifier"></param>
        /// <param name="severity">Notification serverity</param>
        /// <param name="userIds">
        /// Target user id(s).
        /// Used to send notification to specific user(s) (without checking the subscription).
        /// If this is null/empty, the notification is sent to subscribed users.
        /// </param>
        /// <param name="excludedUserIds">
        /// Excluded user id(s).
        /// This can be set to exclude some users while publishing notifications to subscribed users.
        /// It's normally not set if <see cref="userIds"/> is set.
        /// </param>
        /// <returns></returns>
        Task SendAsync(
            string notificationName,
            INotificationData data = null,
            EntityIdentifier entityIdentifier = null,
            NotificationSeverity severity = NotificationSeverity.Info,
            long[] userIds = null,
            long[] excludedUserIds = null);

        /// <summary>
        /// Subscribes to a notification for given user and notification informations.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="notificationName">Name of the notification.</param>
        /// <param name="entityIdentifier">entity identifier</param>
        Task SubscribeAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null);

        /// <summary>
        /// Unsubscribes from a notification.
        /// </summary>
        /// <param name="userId">userId.</param>
        /// <param name="notificationName">Name of the notification.</param>
        /// <param name="entityIdentifier">entity identifier</param>
        Task UnsubscribeAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null);

        /// <summary>
        /// Gets all subscribtions for given notification (including all tenants).
        /// This only works for single database approach in a multitenant application!
        /// </summary>
        /// <param name="notificationName">Name of the notification.</param>
        /// <param name="entityIdentifier">entity identifier</param>
        Task<List<NotificationSubscription>> GetSubscriptionsAsync(string notificationName, EntityIdentifier entityIdentifier = null);

        /// <summary>
        /// Gets subscribed notifications for a user.
        /// </summary>
        /// <param name="userId">User Id.</param>
        Task<List<NotificationSubscription>> GetSubscribedNotificationsAsync(long userId);

        /// <summary>
        /// Checks if a user subscribed for a notification.
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="notificationName">Name of the notification.</param>
        /// <param name="entityIdentifier">entity identifier</param>
        Task<bool> IsSubscribedAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null);
    }
}