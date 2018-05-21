using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL
{
    class VdsContextFactory: IDesignTimeDbContextFactory<VdsContext>
    {
        public VdsContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VdsContext>();
            //var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            builder.UseSqlServer("server=VUTA-HOME\\SQLEXPRESS;Database=Vds;User ID=sa;Password=Echo@1927;Integrated Security=false;MultipleActiveResultSets=true");
            return new VdsContext(builder.Options, null, null);
        }
    }
}
