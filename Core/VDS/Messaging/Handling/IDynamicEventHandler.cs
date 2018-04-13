using System.Threading.Tasks;

namespace VDS.Messaging.Handling
{
    public interface IDynamicEventHandler
    {
        Task HandleAsync(dynamic eventData);
    }
}