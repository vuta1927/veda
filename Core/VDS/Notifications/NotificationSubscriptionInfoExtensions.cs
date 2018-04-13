using System;
using VDS.Domain.Entities;
using VDS.Helpers.Extensions;
using Newtonsoft.Json;

namespace VDS.Notifications
{
    /// <summary>
    /// Extension methods for <see cref="NotificationSubscriptionInfo"/>.
    /// </summary>
    public static class NotificationSubscriptionInfoExtensions
    {
        /// <summary>
        /// Converts <see cref="UserNotificationInfo"/> to <see cref="UserNotification"/>.
        /// </summary>
        public static NotificationSubscription ToNotificationSubscription(this NotificationSubscriptionInfo subscriptionInfo)
        {
            var entityType = subscriptionInfo.EntityTypeAssemblyQualifiedName.IsNullOrEmpty()
                ? null
                : Type.GetType(subscriptionInfo.EntityTypeAssemblyQualifiedName);

            return new NotificationSubscription
            {
                UserId = subscriptionInfo.UserId,
                NotificationName = subscriptionInfo.NotificationName,
                EntityType = entityType,
                EntityTypeName = subscriptionInfo.EntityTypeName,
                EntityId = subscriptionInfo.EntityId.IsNullOrEmpty() ? null : (string)JsonConvert.DeserializeObject(subscriptionInfo.EntityId, EntityHelper.GetPrimaryKeyType(entityType)),
                CreationTime = subscriptionInfo.CreationTime
            };
        }
    }
}