using VDS.Data.Uow;
using VDS.IdentityServer4;
using VDS.IdentityServer4.EntityFrameworkCore;
using VDS.Mapping;
using VDS.Storage.EntityFrameworkCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VDS.Security;
using VDS.Security.Permissions;
using ApiServer.Model;
namespace ApiServer.Model
{
    public class VdsContext : DataContextBase<VdsContext>, IPersistedGrantDbContext
    {
        public VdsContext(DbContextOptions<VdsContext> options, ICurrentUnitOfWorkProvider currentUnitOfWorkProvider, IMediator eventBus)
            : base(options, currentUnitOfWorkProvider, eventBus)
        {
        }
        public DbSet<PersistedGrantEntity> PersistedGrants { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<PermissionRole> PermissionRoles { get; set; }
        public new DbSet<Permission> Permissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<QuantityCheck> QuantityChecks { get; set; }
        public DbSet<QuantityCheckType> QuantityCheckTypes { get; set; }
        public DbSet<ApiServer.Model.Tag> Tags { get; set; }
    }

}
