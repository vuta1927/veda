using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Data.Uow;
using VDS.Helpers.Extensions;
using Microsoft.Extensions.Logging;

namespace VDS.Notifications
{
    public class NotificationDistributer : INotificationDistributer
    {
        private readonly INotificationStore _notificationStore;
        private readonly ILogger<NotificationDistributer> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IEnumerable<INotificationChannel> _notificationChannels;
        private readonly IGuidGenerator _guidGenerator;

        public NotificationDistributer(
            ILogger<NotificationDistributer> logger,
            IUnitOfWorkManager unitOfWorkManager,
            INotificationStore notificationStore,
            IEnumerable<INotificationChannel> notificationChannels,
            IGuidGenerator guidGenerator)
        {
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
            _notificationStore = notificationStore;
            _notificationChannels = notificationChannels;
            _guidGenerator = guidGenerator;
        }

        public async Task DistributeAsync(Guid notificationId)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var notification = await _notificationStore.GetNotificationOrNullAsync(notificationId);
                if (notification == null)
                {
                    _logger.LogWarning(
                        "NotificationDistributerJob cannot continue since could not found notification by id: " +
                        notificationId);
                    return;
                }

                var users = await GetUsers(notification);

                var userNotifications = await SaveUserNotifications(users, notification);

                // await _notificationStore.DeleteNotificationAsync(notification);

                try
                {
                    var notificationType = Type.GetType(notification.NotificationDataTypeAssemblyQualifiedName);
                    foreach (var notificationChannel in _notificationChannels)
                    {
                        if (!notificationChannel.CanHandle(notificationType))
                        {
                            continue;
                        }

                        foreach (var userNotificationInfo in userNotifications)
                        {
                            await notificationChannel.ProcessAsync(userNotificationInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e.ToString(), e);
                }
            });
        }

        protected virtual async Task<long[]> GetUsers(NotificationInfo notificationInfo)
        {
            List<long> userIds;

            if (!notificationInfo.UserIds.IsNullOrEmpty())
            {
                userIds = notificationInfo
                    .UserIds
                    .Split(',')
                    .Select(long.Parse)
                    .ToList();
            }
            else
            {
                //Get subscribed users
                var subscriptions = await _notificationStore.GetSubscriptionsAsync(
                    notificationInfo.NotificationName,
                    notificationInfo.EntityTypeName,
                    notificationInfo.EntityId);

                //Get user ids
                userIds = subscriptions
                    .Select(s => s.UserId)
                    .ToList();
            }

            if (!notificationInfo.ExcludedUserIds.IsNullOrEmpty())
            {
                var excludedUserIds = notificationInfo
                    .ExcludedUserIds
                    .Split(',')
                    .Select(long.Parse)
                    .ToList();

                userIds.RemoveAll(uid => excludedUserIds.Any(euid => euid.Equals(uid)));
            }

            return userIds.ToArray();
        }

        protected virtual async Task<List<UserNotificationInfo>> SaveUserNotifications(long[] userIds, NotificationInfo notificationInfo)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var userNotifications = new List<UserNotificationInfo>();
                foreach (var userId in userIds)
                {
                    var userNotification = new UserNotificationInfo(_guidGenerator.Create())
                    {
                        UserId = userId,
                        NotificationId = notificationInfo.Id
                    };

                    await _notificationStore.InsertUserNotificationAsync(userNotification);
                    userNotifications.Add(userNotification);
                }
                return userNotifications;
            });
        }
    }
}