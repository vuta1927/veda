using System;
using System.Threading.Tasks;
using VDS.Dependency;

namespace VDS.Storage.EntityFrameworkCore
{
    public interface IDbContextSeed : ITransientDependency
    {
        Type ContextType { get; }
        Task SeedAsync();
    }
}