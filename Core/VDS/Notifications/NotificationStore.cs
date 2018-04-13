using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Dependency;
using VDS.Linq.Extensions;

namespace VDS.Notifications
{
    public class NotificationStore : INotificationStore, ITransientDependency
    {
        private readonly IRepository<NotificationInfo, Guid> _notificationRepository;
        private readonly IRepository<UserNotificationInfo, Guid> _userNotificationRepository;
        private readonly IRepository<NotificationSubscriptionInfo, Guid> _notificationSubscriptionRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationStore"/> class.
        /// </summary>
        public NotificationStore(
            IRepository<NotificationInfo, Guid> notificationRepository,
            IRepository<UserNotificationInfo, Guid> userNotificationRepository,
            IRepository<NotificationSubscriptionInfo, Guid> notificationSubscriptionRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _notificationRepository = notificationRepository;
            _userNotificationRepository = userNotificationRepository;
            _notificationSubscriptionRepository = notificationSubscriptionRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task InsertSubscriptionAsync(NotificationSubscriptionInfo subscription)
        {
            await _unitOfWorkManager.PerformAsyncUow<Task>(async () =>
            {
                await _notificationSubscriptionRepository.InsertAsync(subscription);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task DeleteSubscriptionAsync(long userId, string notificationName, string entityTypeName, string entityId)
        {
            await _unitOfWorkManager.PerformAsyncUow<Task>(async () =>
            {
                await _notificationSubscriptionRepository.DeleteAsync(s =>
                    s.UserId == userId &&
                    s.NotificationName == notificationName &&
                    s.EntityTypeName == entityTypeName &&
                    s.EntityId == entityId);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task InsertNotificationAsync(NotificationInfo notification)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                await _notificationRepository.InsertAsync(notification);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task<NotificationInfo> GetNotificationOrNullAsync(Guid notificationId)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () => await _notificationRepository.FirstOrDefaultAsync(notificationId));
        }

        public async Task InsertUserNotificationAsync(UserNotificationInfo userNotification)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                await _userNotificationRepository.InsertAsync(userNotification);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task<List<NotificationSubscriptionInfo>> GetSubscriptionsAsync(string notificationName, string entityTypeName, string entityId)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                return (await _notificationSubscriptionRepository.GetAllListAsync(s =>
                    s.NotificationName == notificationName &&
                    s.EntityTypeName == entityTypeName &&
                    s.EntityId == entityId)).ToList();
            });
        }

        public async Task<List<NotificationSubscriptionInfo>> GetSubscriptionsAsync(long userId)
        {
            return await _unitOfWorkManager.PerformAsyncUow(() =>
                _notificationSubscriptionRepository.GetAllListAsync(s => s.UserId == userId));
        }

        public async Task<bool> IsSubscribedAsync(long userId, string notificationName, string entityTypeName, string entityId)
        {
            return await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                return await _notificationSubscriptionRepository.CountAsync(s =>
                           s.UserId == userId &&
                           s.NotificationName == notificationName &&
                           s.EntityTypeName == entityTypeName &&
                           s.EntityId == entityId
                       ) > 0;
            });
        }

        public async Task UpdateUserNotificationStateAsync(int? tenantId, Guid userNotificationId, UserNotificationState state)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var userNotification = await _userNotificationRepository.FirstOrDefaultAsync(userNotificationId);
                if (userNotification == null)
                {
                    return;
                }

                userNotification.State = state;
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task UpdateAllUserNotificationStatesAsync(long userId, UserNotificationState state)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                var userNotifications = await _userNotificationRepository.GetAllListAsync(un => un.UserId == userId);

                foreach (var userNotification in userNotifications)
                {
                    userNotification.State = state;
                }

                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task DeleteUserNotificationAsync(int? tenantId, Guid userNotificationId)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                await _userNotificationRepository.DeleteAsync(userNotificationId);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public async Task DeleteAllUserNotificationsAsync(long userId)
        {
            await _unitOfWorkManager.PerformAsyncUow(async () =>
            {
                await _userNotificationRepository.DeleteAsync(un => un.UserId == userId);
                await _unitOfWorkManager.Current.SaveChangesAsync();
            });
        }

        public Task<List<UserNotificationInfoWithNotificationInfo>> GetUserNotificationsWithNotificationsAsync(long userId, UserNotificationState? state = null, int skipCount = 0,
            int maxResultCount = int.MaxValue)
        {
            return _unitOfWorkManager.PerformAsyncUow(() =>
            {
                var query = from userNotificationInfo in _userNotificationRepository.GetAll()
                    join notificationInfo in _notificationRepository.GetAll() on userNotificationInfo.NotificationId
                        equals notificationInfo.Id
                    where userNotificationInfo.UserId == userId &&
                          (state == null || userNotificationInfo.State == state.Value)
                    orderby notificationInfo.CreationTime descending
                    select new {userNotificationInfo, notificationInfo = notificationInfo};

                query = query.PageBy(skipCount, maxResultCount);

                var list = query.ToList();

                return Task.FromResult(list.Select(
                    a => new UserNotificationInfoWithNotificationInfo(a.userNotificationInfo, a.notificationInfo)
                ).ToList());
            });
        }

        public async Task<int> GetUserNotificationCountAsync(long userId, UserNotificationState? state = null)
        {
            return await _unitOfWorkManager.PerformAsyncUow(() =>
                _userNotificationRepository.CountAsync(un =>
                    un.UserId == userId && (state == null || un.State == state.Value)));
        }

        public Task DeleteNotificationAsync(NotificationInfo notification)
        {
            return _notificationRepository.DeleteAsync(notification);
        }
    }
}