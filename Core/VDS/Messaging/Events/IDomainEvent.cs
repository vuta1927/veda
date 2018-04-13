using System;
using MediatR;

namespace VDS.Messaging.Events
{
    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        DateTime CreationDate { get; }
    }
}