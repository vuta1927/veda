using System;
using System.Collections.Generic;
using Hangfire;

namespace VDS.BackgroundJobs.Hangfire
{
    public class HangfireIocJobActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public HangfireIocJobActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new HangfireIocJobActivatorScope(this, _serviceProvider);
        }

        class HangfireIocJobActivatorScope : JobActivatorScope
        {
            private readonly JobActivator _activator;
            private readonly IServiceProvider _serviceProvider;

            private readonly List<object> _resolvedObjects;

            public HangfireIocJobActivatorScope(JobActivator activator, IServiceProvider serviceProvider)
            {
                _activator = activator;
                _serviceProvider = serviceProvider;
                _resolvedObjects = new List<object>();
            }

            public override object Resolve(Type type)
            {
                var instance = _activator.ActivateJob(type);
                _resolvedObjects.Add(instance);
                return instance;
            }

            public override void DisposeScope()
            {
            }
        }
    }
}