using System.Collections.Generic;
using VDS.Messaging;
using VDS.Messaging.Events;

namespace VDS.Domain.Entities
{
    public class AggregateRoot : AggregateRoot<int>, IAggregateRoot
    {
        
    }

    /// <summary>
    /// Represents that the derived types are aggregate roots.
    /// </summary>
    public class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>, IPurgeable
//        where TKey : IEquatable<TKey>
    {
        private readonly Queue<IDomainEvent> _uncommittedEvents = new Queue<IDomainEvent>();
        
        public IEnumerable<IDomainEvent> UncommittedEvents => _uncommittedEvents;
        public void Replay(IEnumerable<IDomainEvent> events)
        {
            ((IPurgeable)this).Purge();
            foreach (var evnt in events)
            {
                ApplyEvent(evnt);
            }
        }

        protected void ApplyEvent<TEvent>(TEvent evnt) where TEvent : IDomainEvent
        {
            _uncommittedEvents.Enqueue(evnt);
        }

        public void Purge()
        {
            if (_uncommittedEvents.Count > 0)
                _uncommittedEvents.Clear();
        }
    }
}