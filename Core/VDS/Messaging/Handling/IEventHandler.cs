using VDS.Dependency;
using VDS.Messaging.Events;

namespace VDS.Messaging.Handling
{
    /// <summary>
    /// Marker interface that makes it easier to discover handlers via reflection.
    /// </summary>
    public interface IEventHandler : ITransientDependency { }

    public interface IEventHandler<T> : IEventHandler, IHandler<T>
        where T : IEvent
    {
    }
}