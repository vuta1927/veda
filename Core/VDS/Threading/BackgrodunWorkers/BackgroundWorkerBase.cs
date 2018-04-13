using Microsoft.Extensions.Logging;

namespace VDS.Threading.BackgrodunWorkers
{
    /// <summary>
    ///     Base class that can be used to implement <see cref="IBackgroundWorker" />.
    /// </summary>
    public abstract class BackgroundWorkerBase : RunnableBase, IBackgroundWorker
    {
        protected readonly ILogger Logger;

        /// <summary>
        ///     Constructor.
        /// </summary>
        protected BackgroundWorkerBase(ILogger logger)
        {
            Logger = logger;
        }

        public override void Start()
        {
            base.Start();
            Logger.LogDebug("Start background worker: " + ToString());
        }

        public override void Stop()
        {
            base.Stop();
            Logger.LogDebug("Stop background worker: " + ToString());
        }

        public override void WaitToStop()
        {
            base.WaitToStop();
            Logger.LogDebug("WaitToStop background worker: " + ToString());
        }

        public override string ToString()
        {
            return GetType().FullName;
        }
    }
}