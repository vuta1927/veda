using System;
using VDS.Data.Uow;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Storage.EntityFrameworkCore
{
    public interface IEfGenericRepositoryRegistrar
    {
        void RegisterForDbContext(Type dbContextType, IServiceCollection services, AutoRepositoryTypesAttribute defaultAutoRepositoryTypesAttribute);
    }
}