using System;
using VDS.Configuration;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Configuration
{
    public interface IEfCoreConfiguration : IConfigurator
    {
        void AddDbContext<TDbContext>(Action<DbContextConfiguration<TDbContext>> action)
            where TDbContext : DbContext;
    }
}