using VDS.Dependency;
using VDS.Messaging;
using VDS.Messaging.Events;
using Hangfire.Common;
using Hangfire.Server;

namespace VDS.BackgroundJobs.Hangfire
{
    public class HangfireJobExceptionFilter : JobFilterAttribute, IServerFilter, ITransientDependency
    {
        private readonly IEventBus _eventBus;

        public HangfireJobExceptionFilter(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (filterContext.Exception != null)
            {
                _eventBus.Publish(new BackgroundJobExceptionEvent(
                    "A background job execution is failed on Hangfire. See inner exception for details. Use JobObject to get Hangfire background job object.",
                    filterContext.Exception)
                {
                    JobObject = filterContext.BackgroundJob
                });
            }
        }
    }
}