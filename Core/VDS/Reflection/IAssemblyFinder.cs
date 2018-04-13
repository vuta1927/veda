using System.Collections.Generic;
using System.Reflection;

namespace VDS.Reflection
{
    /// <summary>
    /// This interface is used to get assemblies in the application.
    /// </summary>
    public interface IAssemblyFinder
    {
        /// <summary>
        /// Gets all assemblies.
        /// </summary>
        /// <returns>List of assemblies</returns>
        List<Assembly> GetAllAssemblies();
    }
}