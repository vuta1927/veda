using AuthServer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VDS.Data.Uow;
using VDS.IdentityServer4;
using VDS.IdentityServer4.EntityFrameworkCore;
using VDS.Security;
using VDS.Security.Permissions;
using VDS.Storage.EntityFrameworkCore;

namespace AuthServer
{
    public class VdsContext: DataContextBase<VdsContext>, IPersistedGrantDbContext
    {
        public VdsContext(DbContextOptions<VdsContext> options, ICurrentUnitOfWorkProvider currentUnitOfWorkProvider, IMediator eventBus) 
            : base(options, currentUnitOfWorkProvider, eventBus)
        { }

        public DbSet<PersistedGrantEntity> PersistedGrants { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<PermissionRole> PermissionRoles { get; set; }
        public new DbSet<Permission> Permissions { get; set; }
    }
}
