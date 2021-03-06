﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthServer
{
    class VdsContextFactory: IDesignTimeDbContextFactory<VdsContext>
    {
        public VdsContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VdsContext>();
            //var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            builder.UseSqlServer("server=localhost;Database=Vds;User ID=sa;Password=Echo@1927;Integrated Security=false;MultipleActiveResultSets=true");
            return new VdsContext(builder.Options, null, null);
        }
    }
}
