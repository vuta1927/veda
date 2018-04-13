using VDS.Configuration;

namespace VDS.BackgroundJobs
{
    internal class BackgroundJobConfiguration : IBackgroundJobConfiguration
    {
        public bool IsJobExecutionEnabled { get; set; }
        public IConfigure Configure { get; }

        public BackgroundJobConfiguration(IConfigure configure)
        {
            IsJobExecutionEnabled = true;
            Configure = configure;
        }
    }
}