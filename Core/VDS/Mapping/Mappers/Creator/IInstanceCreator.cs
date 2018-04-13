using System.Reflection.Emit;
using VDS.Mapping.Runtime;

namespace VDS.Mapping.Mappers.Creator
{
    internal interface IInstanceCreator<TTarget>
    {
        void Compile(ModuleBuilder builder);

        void Emit(CompilationContext context);
    }
}
