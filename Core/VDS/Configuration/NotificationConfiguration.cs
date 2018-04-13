namespace VDS.Configuration
{
    public class NotificationConfiguration : INotificationConfiguration
    {
        public NotificationConfiguration(IConfigure configure)
        {
            Configure = configure;
        }

        public IConfigure Configure { get; }
    }
}