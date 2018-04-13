using System;
using VDS.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Storage.EntityFrameworkCore.Configuration
{
    public class EfCoreConfiguration : IEfCoreConfiguration
    {
        private readonly IDbContextTypeMatcher _dbContextTypeMatcher;

        public EfCoreConfiguration(IConfigure configure, IDbContextTypeMatcher dbContextTypeMatcher)
        {
            Configure = configure;
            _dbContextTypeMatcher = dbContextTypeMatcher;
        }

        public void AddDbContext<TDbContext>(Action<DbContextConfiguration<TDbContext>> action) 
            where TDbContext : DbContext
        {
            var dbOptions = new DbContextConfiguration<TDbContext>(string.Empty, null);
            action(dbOptions);
            Configure.Services.AddSingleton(dbOptions.DbContextOptions.Options);
            Configure.Services.AddSingleton(typeof(IDbContextConfigurer<TDbContext>),
                new DbContextConfigurerAction<TDbContext>(action));
            _dbContextTypeMatcher.Populate(new []{typeof(TDbContext)});
        }

        public IConfigure Configure { get; }
    }
}