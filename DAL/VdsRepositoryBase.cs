using VDS.Data.Uow;
using VDS.Domain.Entities;
using VDS.Storage.EntityFrameworkCore;
using VDS.Storage.EntityFrameworkCore.Repositories;

namespace DAL
{
    /// <summary>
    /// Base class for custom repositories of the application.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
    public class VdsRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<VdsContext, TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        public VdsRepositoryBase(IDbContextProvider<VdsContext> dbContextProvider, IUnitOfWorkManager unitOfWorkManager)
            : base(dbContextProvider, unitOfWorkManager)
        {
        }
    }

    /// <summary>
    /// Base class for custom repositories of the application.
    /// This is a shortcut of <see cref="DemoRepositoryBase{TEntity,TPrimaryKey}"/> for <see cref="int"/> primary key.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public abstract class DemoRepositoryBase<TEntity> : VdsRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected DemoRepositoryBase(IDbContextProvider<VdsContext> dbContextProvider, IUnitOfWorkManager unitOfWorkManager)
            : base(dbContextProvider, unitOfWorkManager)
        {
        }
    }
}
