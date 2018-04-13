using System;
using System.Threading.Tasks;

namespace VDS.Notifications
{
    public interface INotificationChannel
    {
        bool CanHandle(Type notificationDataType);
        Task ProcessAsync(UserNotificationInfo userNotificationsInfo);
    }
}