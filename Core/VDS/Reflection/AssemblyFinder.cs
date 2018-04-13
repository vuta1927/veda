using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Extensions;

namespace VDS.Reflection
{
    public class AssemblyFinder : IAssemblyFinder
    {
        public List<Assembly> GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetExcutingAssembiles().ToList();
        }
    }
}