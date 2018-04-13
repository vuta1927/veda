using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.BackgroundJobs;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Domain.Entities;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using VDS.Json;

namespace VDS.Notifications
{
    public class NotificationService : INotificationService, ITransientDependency
    {
        public const int MaxUserCountToDirectlySendANotification = 5;

        private readonly INotificationDistributer _notificationDistributer;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly INotificationStore _notificationStore;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IGuidGenerator _guidGenerator;

        public NotificationService(
            INotificationDistributer notificationDistributer,
            IUnitOfWorkManager unitOfWorkManager,
            IBackgroundJobManager backgroundJobManager,
            INotificationStore notificationStore,
            IGuidGenerator guidGenerator)
        {
            _notificationDistributer = notificationDistributer;
            _unitOfWorkManager = unitOfWorkManager;
            _backgroundJobManager = backgroundJobManager;
            _notificationStore = notificationStore;
            _guidGenerator = guidGenerator;
        }

        public async Task SendAsync(
            string notificationName,
            INotificationData data = null,
            EntityIdentifier entityIdentifier = null,
            NotificationSeverity severity = NotificationSeverity.Info,
            long[] userIds = null,
            long[] excludedUserIds = null)
        {
            Throw.IfArgumentNull(notificationName, nameof(notificationName));

            var notification = new NotificationInfo(_guidGenerator.Create())
            {
                NotificationName = notificationName,
                NotificationDataTypeAssemblyQualifiedName = data == null ? typeof(NotificationData).AssemblyQualifiedName : data.GetType().AssemblyQualifiedName,
                EntityTypeName = entityIdentifier?.Type.FullName,
                EntityTypeAssemblyQualifiedName = entityIdentifier?.Type.AssemblyQualifiedName,
                EntityId = entityIdentifier?.Id.ToJsonString(),
                Severity = severity,
                UserIds = userIds.IsNullOrEmpty() ? null : userIds.Select(uid => uid).JoinAsString(","),
                ExcludedUserIds = excludedUserIds.IsNullOrEmpty() ? null : excludedUserIds.Select(uid => uid).JoinAsString(","),
                Data = data?.ToJsonString(),
                DataTypeName = data?.GetType().AssemblyQualifiedName
            };

            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                await _notificationStore.InsertNotificationAsync(notification);

                // To get Id of the notification
                await _unitOfWorkManager.Current.SaveChangesAsync();

                if (userIds != null && userIds.Length <= MaxUserCountToDirectlySendANotification)
                {
                    // We can process the notification since there are not much receivers
                    await _notificationDistributer.DistributeAsync(notification.Id);
                }
                else
                {
                    await _backgroundJobManager
                        .EnqueueAsync<NotificationDistributerJob, NotificationDistributerJobArgs>(
                            new NotificationDistributerJobArgs(notification.Id));
                }
            });
        }

        public async Task SubscribeAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null)
        {
            if (await IsSubscribedAsync(userId, notificationName, entityIdentifier))
            {
                return;
            }

            await _notificationStore.InsertSubscriptionAsync(
                new NotificationSubscriptionInfo(
                    _guidGenerator.Create(),
                    userId,
                    notificationName,
                    entityIdentifier));
        }

        public async Task UnsubscribeAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null)
        {
            await _notificationStore.DeleteSubscriptionAsync(
                userId,
                notificationName,
                entityIdentifier?.Type.FullName,
                entityIdentifier?.Id.ToJsonString());
        }

        public async Task<List<NotificationSubscription>> GetSubscriptionsAsync(string notificationName, EntityIdentifier entityIdentifier = null)
        {
            var notificationSubscriptionInfos = await _notificationStore.GetSubscriptionsAsync(
                notificationName,
                entityIdentifier?.Type.FullName,
                entityIdentifier?.Id.ToJsonString()
            );

            return notificationSubscriptionInfos
                .Select(nsi => nsi.ToNotificationSubscription())
                .ToList();
        }

        public async Task<List<NotificationSubscription>> GetSubscribedNotificationsAsync(long userId)
        {
            var notificationSubscriptionInfos = await _notificationStore.GetSubscriptionsAsync(userId);

            return notificationSubscriptionInfos
                .Select(nsi => nsi.ToNotificationSubscription())
                .ToList();
        }

        public async Task<bool> IsSubscribedAsync(long userId, string notificationName, EntityIdentifier entityIdentifier = null)
        {
            return await _notificationStore.IsSubscribedAsync(
                userId,
                notificationName,
                entityIdentifier?.Type.FullName,
                entityIdentifier?.Id.ToJsonString());
        }
    }
}