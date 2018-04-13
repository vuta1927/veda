using Microsoft.EntityFrameworkCore;

namespace VDS.IdentityServer4.EntityFrameworkCore
{
    public interface IPersistedGrantDbContext
    {
        DbSet<PersistedGrantEntity> PersistedGrants { get; set; }
    }
}
