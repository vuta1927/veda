﻿using System;
using System.Reflection;
using System.Reflection.Emit;
using VDS.Mapping.Runtime;

namespace VDS.Mapping.Mappers.ValueMapper
{
    internal abstract class ValueMapper
    {
        public abstract void Compile(ModuleBuilder builder);

        public abstract void Emit(Type sourceType, Type targetType, CompilationContext context);

        public virtual Delegate CreateDelegate(Type sourceType, Type targetType, ModuleBuilder builder)
        {
            var typeBuilder = builder.DefineStaticType();
            var methodBuilder = typeBuilder.DefineStaticMethod("Map");
            methodBuilder.SetParameters(sourceType, targetType);
            var il = methodBuilder.GetILGenerator();
            var context = new CompilationContext(il);
            context.SetSource(purpose => il.Emit(OpCodes.Ldarg_0));
            context.SetTarget(purpose => il.Emit(OpCodes.Ldarg_1));
            Emit(sourceType, targetType, context);
            context.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo()
                .GetMethod("Map", BindingFlags.Static | BindingFlags.Public)
                .CreateDelegate(typeof(Action<,>).MakeGenericType(sourceType, targetType));
        }
    }
}
