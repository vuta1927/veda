using System;
using System.Threading.Tasks;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Dependency;

namespace VDS.Messaging.Events
{
    public class EventService : IEventService, ISingletonDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<EventLogEntry, Guid> _eventLogEntryRepository;

        public EventService(IUnitOfWorkManager unitOfWorkManager, IRepository<EventLogEntry, Guid> eventLogEntryRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _eventLogEntryRepository = eventLogEntryRepository;
        }

        public Task SaveEventAsync(IEvent @event)
        {
            var eventLogEntry = new EventLogEntry(@event);
            _unitOfWorkManager.PerformSyncUow(() => _eventLogEntryRepository.Insert(eventLogEntry));
            return Task.FromResult(0);
        }

        public Task MarkEventAsPublishedAsync(IEvent @event)
        {
            var eventLogEntry = _eventLogEntryRepository.Single(ie => ie.EventId == @event.Id);
            eventLogEntry.TimesSend++;
            eventLogEntry.State = EventStateEnum.Published;

            _unitOfWorkManager.PerformSyncUow(() => _eventLogEntryRepository.Update(eventLogEntry));

            return Task.FromResult(0);
        }
    }
}