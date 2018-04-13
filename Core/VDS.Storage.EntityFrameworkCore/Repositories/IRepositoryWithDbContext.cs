using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Repositories
{
    public interface IRepositoryWithDbContext
    {
        DbContext GetDbContext();
    }
}