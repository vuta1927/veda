using System;
using System.Collections.Generic;
using VDS.Domain.Entities;

namespace VDS.Storage.EntityFrameworkCore
{
    public interface IDbContextEntityFinder
    {
        IEnumerable<EntityTypeInfo> GetEntityTypeInfos(Type dbContextType);
    }
}