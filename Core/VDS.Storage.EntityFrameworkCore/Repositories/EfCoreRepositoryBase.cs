using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Repositories
{
    public class EfCoreRepositoryBase<TDbContext, TEntity> : EfCoreRepositoryBase<TDbContext, TEntity, int>, IRepository<TEntity>
        where TEntity : class, IEntity<int>
        where TDbContext : DbContext
    {
        public EfCoreRepositoryBase(IDbContextProvider<TDbContext> dbContextProvider, IUnitOfWorkManager unitOfWorkManager) 
            : base(dbContextProvider, unitOfWorkManager)
        {
        }
    }
}