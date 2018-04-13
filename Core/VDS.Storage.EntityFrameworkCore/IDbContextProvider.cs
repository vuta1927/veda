using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore
{
    public interface IDbContextProvider<out TDbContext>
        where TDbContext : DbContext
    {
        TDbContext GetDbContext();
    }
}