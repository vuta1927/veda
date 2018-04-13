using VDS.Messaging.Commands;

namespace VDS.Messaging.Handling
{
    /// <summary>
    /// Marker interface that makes it easier to discover handlers via reflection.
    /// </summary>
    public interface ICommandHandler { }

    public interface ICommandHandler<T> : ICommandHandler, IHandler<T>
        where T : ICommand
    {
        
    }
}