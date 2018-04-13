using System;
using System.Reflection;
using System.Reflection.Emit;

namespace VDS.Mapping.Runtime
{
    internal class ActionInvokerBuilder<TSource, TTarget> : IInvokerBuilder
    {
        private readonly Action<TSource, TTarget> _action;
        private MethodInfo _invokeMethod;
        private static readonly MethodInfo _actionInvokeMethod;

        static ActionInvokerBuilder()
        {
            _actionInvokeMethod = typeof(Action<TSource, TTarget>).GetTypeInfo().GetMethod("Invoke");
        }

        public ActionInvokerBuilder(Action<TSource, TTarget> action)
        {
            _action = action;
        }

        public MethodInfo MethodInfo => _invokeMethod;

        public void Compile(ModuleBuilder builder)
        {
            var typeBuilder = builder.DefineStaticType();
            var field = typeBuilder.DefineStaticField<Action<TSource, TTarget>>("Target");
            var methodBuilder = typeBuilder.DefineStaticMethod("Invoke");
            methodBuilder.SetParameters(typeof(TSource), typeof(TTarget));

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, field);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, _actionInvokeMethod);
            il.Emit(OpCodes.Ret);
            var type = typeBuilder.CreateTypeInfo();
            type.GetField("Target").SetValue(null, _action);
            _invokeMethod = type.GetMethod("Invoke");
        }

        public void Emit(CompilationContext context)
        {
            context.EmitCall(_invokeMethod);
            context.CurrentType = null;
        }
    }
}
