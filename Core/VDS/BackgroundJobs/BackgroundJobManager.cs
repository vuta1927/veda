using System;
using System.Reflection;
using System.Threading.Tasks;
using VDS.Dependency;
using VDS.Json;
using VDS.Messaging;
using VDS.Messaging.Events;
using VDS.Serialization;
using VDS.Threading;
using VDS.Threading.BackgrodunWorkers;
using VDS.Threading.Timers;
using VDS.Timing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace VDS.BackgroundJobs
{
    public class BackgroundJobManager : PeriodicBackgroundWorkerBase, IBackgroundJobManager, ISingletonDependency
    {
        private readonly IEventBus _eventBus;

        /// <summary>
        /// Interval between polling jobs from <see cref="IBackgroundJobStore"/>.
        /// Default value: 5000 (5 seconds).
        /// </summary>
        public static int JobPollPeriod { get; set; }

        private readonly IServiceProvider _serviceProvider;
        private readonly IBackgroundJobStore _store;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;

        static BackgroundJobManager()
        {
            JobPollPeriod = 5000;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobManager"/> class.
        /// </summary>
        public BackgroundJobManager(
            IServiceProvider serviceProvider,
            IBackgroundJobStore store,
            DomainTimer timer, 
            IEventBus eventBus,
            ISerializer serializer,
            ILogger<BackgroundJobManager> logger)
            : base(timer, logger)
        {
            _store = store;
            _eventBus = eventBus;
            _serviceProvider = serviceProvider;
            _serializer = serializer;
            _logger = logger;

            Timer.Period = JobPollPeriod;
        }

        public async Task<string> EnqueueAsync<TJob, TArgs>(TArgs args, BackgroundJobPriority priority = BackgroundJobPriority.Normal, TimeSpan? delay = null)
            where TJob : IBackgroundJob<TArgs>
        {
            var jobInfo = new BackgroundJobInfo
            {
                JobType = typeof(TJob).AssemblyQualifiedName,
                JobArgs = args.ToJsonString(),
                Priority = priority
            };

            if (delay.HasValue)
            {
                jobInfo.NextTryTime = Clock.Now.Add(delay.Value);
            }

            await _store.InsertAsync(jobInfo);

            return jobInfo.Id.ToString();
        }

        public async Task<bool> DeleteAsync(string jobId)
        {
            if (long.TryParse(jobId, out long finalJobId) == false)
            {
                throw new ArgumentException($"The jobId '{jobId}' should be a number.", nameof(jobId));
            }

            var jobInfo = await _store.GetAsync(finalJobId);
            if (jobInfo == null)
            {
                return false;
            }

            await _store.DeleteAsync(jobInfo);
            return true;
        }

        protected override void DoWork()
        {
            var waitingJobs = AsyncHelper.RunSync(() => _store.GetWaitingJobsAsync(1000));

            foreach (var job in waitingJobs)
            {
                TryProcessJob(job);
            }
        }

        private void TryProcessJob(BackgroundJobInfo jobInfo)
        {
            try
            {
                jobInfo.TryCount++;
                jobInfo.LastTryTime = Clock.Now;

                var jobType = Type.GetType(jobInfo.JobType);
                var job = _serviceProvider.GetService(jobType);
                try
                {
                    var jobExecuteMethod = job.GetType().GetTypeInfo().GetMethod("Execute");
                    var argsType = jobExecuteMethod.GetParameters()[0].ParameterType;
//                    var argsObj = _serializer.Deserialize(jobInfo.JobArgs, argsType);
                    var argsObj = JsonConvert.DeserializeObject(jobInfo.JobArgs, argsType);

                    jobExecuteMethod.Invoke(job, new[] { argsObj });

                    AsyncHelper.RunSync(() => _store.DeleteAsync(jobInfo));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message, ex);

                    var nextTryTime = jobInfo.CalculateNextTryTime();
                    if (nextTryTime.HasValue)
                    {
                        jobInfo.NextTryTime = nextTryTime.Value;
                    }
                    else
                    {
                        jobInfo.IsAbandoned = true;
                    }

                    TryUpdate(jobInfo);

                    _eventBus.Publish(
                        new HandleEventException(
                            new BackgroundJobException(
                                "A background job execution is failed. See inner exception for details. See BackgroundJob property to get information on the background job.",
                                ex
                            )
                            {
                                BackgroundJob = jobInfo,
                                JobObject = job
                            }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.ToString(), ex);

                jobInfo.IsAbandoned = true;

                TryUpdate(jobInfo);
            }
        }

        private void TryUpdate(BackgroundJobInfo jobInfo)
        {
            try
            {
                _store.UpdateAsync(jobInfo);
            }
            catch (Exception updateEx)
            {
                _logger.LogWarning(updateEx.ToString(), updateEx);
            }
        }
    }
}