using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Configuration
{
    public class DbContextConfiguration<TDbContext>
        where TDbContext : DbContext
    {
        public string ConnectionString { get; set; }
        public DbConnection ExistingConnection { get; set; }
        public DbContextOptionsBuilder<TDbContext> DbContextOptions { get; set; }

        public DbContextConfiguration(string connectionString, DbConnection existingConnection)
        {
            ConnectionString = connectionString;
            ExistingConnection = existingConnection;

            DbContextOptions = new DbContextOptionsBuilder<TDbContext>();
        }
    }
}