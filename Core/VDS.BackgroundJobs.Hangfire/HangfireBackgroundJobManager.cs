using System;
using System.Threading.Tasks;
using VDS.BackgroundJobs.Hangfire.Configuration;
using VDS.Helpers.Exception;
using VDS.Threading.BackgrodunWorkers;
using Hangfire;
using Microsoft.Extensions.Logging;
using HangfireBackgroundJob = Hangfire.BackgroundJob;

namespace VDS.BackgroundJobs.Hangfire
{
    public class HangfireBackgroundJobManager : BackgroundWorkerBase, IBackgroundJobManager
    {
        private readonly IBackgroundJobConfiguration _backgroundJobConfiguration;
        private readonly IHangfireConfiguration _hangfireConfiguration;
        private readonly ILogger _logger;

        public HangfireBackgroundJobManager(
            IHangfireConfiguration hangfireConfiguration, 
            IBackgroundJobConfiguration backgroundJobConfiguration, 
            ILogger<HangfireBackgroundJobManager> logger)
            : base(logger)
        {
            _hangfireConfiguration = hangfireConfiguration;
            _backgroundJobConfiguration = backgroundJobConfiguration;
            _logger = logger;
        }

        public override void Start()
        {
            base.Start();

            if (_hangfireConfiguration.Server == null && _backgroundJobConfiguration.IsJobExecutionEnabled)
            {
                _hangfireConfiguration.Server = new BackgroundJobServer();
            }
        }

        public override void WaitToStop()
        {
            if (_hangfireConfiguration.Server != null)
            {
                try
                {
                    _hangfireConfiguration.Server.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.ToString(), ex);
                }
            }

            base.WaitToStop();
        }

        public Task<string> EnqueueAsync<TJob, TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal,
            TimeSpan? delay = null) where TJob : IBackgroundJob<TArgs>
        {
            var jobUniqueIdentifier = !delay.HasValue
                ? HangfireBackgroundJob.Enqueue<TJob>(job => job.Execute(args))
                : HangfireBackgroundJob.Schedule<TJob>(job => job.Execute(args), delay.Value);

            return Task.FromResult(jobUniqueIdentifier);
        }

        public Task<bool> DeleteAsync(string jobId)
        {
            Throw.IfArgumentNull(jobId, nameof(jobId));
            var successfulDeletion = HangfireBackgroundJob.Delete(jobId);
            return Task.FromResult(successfulDeletion);
        }
    }
}