namespace VDS.BackgroundJobs
{
    /// <summary>
    /// Base class that can be used to implement <see cref="IBackgroundJob{TArgs}"/>.
    /// </summary>
    public abstract class BackgroundJob<TArgs> : IBackgroundJob<TArgs>
    {
        public abstract void Execute(TArgs args);
    }
}