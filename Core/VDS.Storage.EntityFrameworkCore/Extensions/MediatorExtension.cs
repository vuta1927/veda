using System.Linq;
using System.Threading.Tasks;
using VDS.Domain.Entities;
using MediatR;

namespace VDS.Storage.EntityFrameworkCore.Extensions
{
    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, DataContext ctx)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<IGeneratesDomainEvents>()
                .Where(x => x.Entity.UncommittedEvents != null && x.Entity.UncommittedEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.UncommittedEvents)
                .ToList();

            domainEntities.ForEach(entity => (entity as IPurgeable)?.Purge());

            var tasks = domainEvents
                .Select(async domainEvent => { await mediator.Publish(domainEvent); });

            await Task.WhenAll(tasks);
        }
    }
}