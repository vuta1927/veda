using System.Collections.Generic;
using VDS.Messaging;
using VDS.Messaging.Events;

namespace VDS.Domain.Entities
{
    public interface IAggregateRoot : IAggregateRoot<int>, IEntity
    {
        
    }

    /// <summary>
    /// Represents that the implemented classes are aggregate roots.
    /// </summary>
    /// <typeparam name="TKey">The type of the aggregate root key.</typeparam>
    /// <seealso cref="IEntity{TKey}" />
    public interface IAggregateRoot<TKey> : IGeneratesDomainEvents, IEntity<TKey>
//        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Reapplies the events on current aggregate root.
        /// </summary>
        /// <param name="events">The events.</param>
        void Replay(IEnumerable<IDomainEvent> events);
    }

    public interface IGeneratesDomainEvents
    {
        /// <summary>
        /// Gets the uncommitted events stored in current aggregate root.
        /// </summary>
        /// <value>
        /// The uncommitted events.
        /// </value>
        IEnumerable<IDomainEvent> UncommittedEvents { get; }
    }
}