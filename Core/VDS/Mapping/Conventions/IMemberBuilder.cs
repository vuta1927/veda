using VDS.Mapping.Runtime;

namespace VDS.Mapping.Conventions
{
    internal interface IMemberBuilder
    {
        void EmitGetter(CompilationContext context);

        void EmitSetter(CompilationContext context);
    }
}
