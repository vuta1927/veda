using System;

namespace VDS.Messaging.Events
{
    public interface IEvent
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
    }
}