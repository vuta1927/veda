using System.Reflection.Emit;

namespace VDS.Mapping.Runtime
{
    internal interface IInvokerBuilder
    {
        void Compile(ModuleBuilder builder);

        void Emit(CompilationContext context);
    }
}
