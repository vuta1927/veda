using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Configuration
{
    public interface IDbContextConfigurer<TDbContext>
        where TDbContext : DbContext
    {
        void Configure(DbContextConfiguration<TDbContext> configuration);
    }
}