using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Reflection;

namespace VDS.Helpers.Extensions
{
    public static class AppDomainExtensions
    {
        private static string GetActualDomainPath(this AppDomain @this)
        {
            return @this.RelativeSearchPath ?? @this.BaseDirectory;
        }

        private static IEnumerable<Assembly> _excutingAssembiles;

        public static IEnumerable<Assembly> GetExcutingAssembiles(this AppDomain @this, Type type = null)
        {
            type = type ?? typeof(AppDomainExtensions);

            if (_excutingAssembiles == null || !_excutingAssembiles.Any())
                _excutingAssembiles = ReflectionUtil.GetAssemblies(new AssemblyFilter(@this.GetActualDomainPath())).Where(assembly =>
                    !assembly.IsDynamic && assembly.FullName == type.AssemblyQualifiedName.Replace(type.FullName + ", ", "")
                    || assembly.GetReferencedAssemblies().Any(ass => ass.FullName == type.AssemblyQualifiedName.Replace(type.FullName + ", ", ""))
                ).ToList();

            return _excutingAssembiles;
        }
    }
}