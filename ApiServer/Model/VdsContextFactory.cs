﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiServer.Model
{
    class VdsContextFactory: IDesignTimeDbContextFactory<VdsContext>
    {
        public VdsContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<VdsContext>();

            builder.UseSqlServer("server=localhost;Database=Vds;User ID=sa;Password=Echo@1927;Integrated Security=false;MultipleActiveResultSets=true");
            return new VdsContext(builder.Options, null, null);
        }
    }
}
