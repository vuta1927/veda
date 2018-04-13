﻿namespace VDS.Threading
{
    /// <summary>
    ///     Base implementation of <see cref="IRunnable" />.
    /// </summary>
    public abstract class RunnableBase : IRunnable
    {
        private volatile bool _isRunning;

        /// <summary>
        ///     A boolean value to control the running.
        /// </summary>
        public bool IsRunning => _isRunning;

        public virtual void Start()
        {
            _isRunning = true;
        }

        public virtual void Stop()
        {
            _isRunning = false;
        }

        public virtual void WaitToStop()
        {
        }
    }
}