using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Dependency;

namespace VDS.Threading.BackgrodunWorkers
{
    public class BackgroundWorkerManager : RunnableBase, IBackgroundWorkerManager, IDisposable, ISingletonDependency
    {
        private readonly List<IBackgroundWorker> _backgroundWorkers;

        public BackgroundWorkerManager()
        {
            _backgroundWorkers = new List<IBackgroundWorker>();
        }

        public override void Start()
        {
            base.Start();
            _backgroundWorkers.ForEach(a => a.Start());
        }

        public override void Stop()
        {
            _backgroundWorkers.ForEach(a => a.Stop());
            base.Stop();
        }

        public override void WaitToStop()
        {
            _backgroundWorkers.ForEach(a => a.WaitToStop());
            base.WaitToStop();
        }

        public void Add(IBackgroundWorker worker)
        {
            _backgroundWorkers.Append(worker);
            if (IsRunning)
                worker.Start();
        }

        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _backgroundWorkers.Clear();
        }
    }
}