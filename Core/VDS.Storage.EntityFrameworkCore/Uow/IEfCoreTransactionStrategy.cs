using VDS.Data.Uow;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Uow
{
    public interface IEfCoreTransactionStrategy
    {
        void InitOptions(UnitOfWorkOptions options);

        DbContext CreateDbContext<TDbContext>(string connectionString, IDbContextResolver dbContextResolver)
            where TDbContext : DbContext;

        void Commit();

        void Dispose();
    }
}