using System;
using System.ComponentModel.DataAnnotations;
using VDS.Domain.Entities;
using VDS.Domain.Entities.Auditing;
using VDS.Json;

namespace VDS.Notifications
{
    public class NotificationSubscriptionInfo : CreationAuditedEntity<Guid>
    {

        /// <summary>
        /// User Id.
        /// </summary>
        public virtual long UserId { get; set; }

        /// <summary>
        /// Notification unique name.
        /// </summary>
        [MaxLength(NotificationInfo.MaxNotificationNameLength)]
        public virtual string NotificationName { get; set; }

        /// <summary>
        /// Gets/sets entity type name, if this is an entity level notification.
        /// It's FullName of the entity type.
        /// </summary>
        [MaxLength(NotificationInfo.MaxEntityTypeNameLength)]
        public virtual string EntityTypeName { get; set; }

        /// <summary>
        /// AssemblyQualifiedName of the entity type.
        /// </summary>
        [MaxLength(NotificationInfo.MaxEntityTypeAssemblyQualifiedNameLength)]
        public virtual string EntityTypeAssemblyQualifiedName { get; set; }

        /// <summary>
        /// Gets/sets primary key of the entity, if this is an entity level notification.
        /// </summary>
        [MaxLength(NotificationInfo.MaxEntityIdLength)]
        public virtual string EntityId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSubscriptionInfo"/> class.
        /// </summary>
        public NotificationSubscriptionInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSubscriptionInfo"/> class.
        /// </summary>
        public NotificationSubscriptionInfo(Guid id, long userId, string notificationName, EntityIdentifier entityIdentifier = null)
        {
            Id = id;
            NotificationName = notificationName;
            UserId = userId;
            EntityTypeName = entityIdentifier?.Type.FullName;
            EntityTypeAssemblyQualifiedName = entityIdentifier?.Type.AssemblyQualifiedName;
            EntityId = entityIdentifier?.Id.ToJsonString();
        }
    }
}