using System.Threading.Tasks;

namespace VDS.Messaging.Events
{
    public interface IEventService
    {
        Task SaveEventAsync(IEvent @event);
        Task MarkEventAsPublishedAsync(IEvent @event);
    }
}