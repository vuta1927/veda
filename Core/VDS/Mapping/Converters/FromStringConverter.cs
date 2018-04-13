using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VDS.Mapping.Runtime;

namespace VDS.Mapping.Converters
{
    internal class FromStringConverter : ValueConverter
    {
        private static readonly MethodInfo _enumParseMethod;
        private static readonly MethodInfo _checkEmptyMethod;
        private static readonly MethodInfo _stringTrimMethod;

        static FromStringConverter()
        {
            Func<MethodInfo,bool> enumParsePredicate = method =>
            {
                if (method.Name == "Parse")
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 2 && parameters[0].ParameterType == typeof (Type) && parameters[1].ParameterType == typeof (string);
                }
                return false;
            };
            Func<MethodInfo, bool> stringTrimPredicate = method => method.Name == "Trim" && method.GetParameters().Length == 0;
            Func<MethodInfo, bool> checkEmptyPredicate = method =>
            {
                if (method.Name == "IsNullOrWhiteSpace")
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof (string);
                }
                return false;
            };

            _enumParseMethod = typeof(Enum).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static).Where(enumParsePredicate).FirstOrDefault();
            _stringTrimMethod = typeof(string).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(stringTrimPredicate);
            _checkEmptyMethod = typeof(string).GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(checkEmptyPredicate);

            _checkEmptyMethod = typeof(string).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(checkEmptyPredicate);
        }
        
        private static readonly ConcurrentDictionary<Type, MethodInfo> _methods = new ConcurrentDictionary<Type, MethodInfo>();

        private static MethodInfo GetConvertMethod(Type targetType)
        {
            return _methods.GetOrAdd(targetType, FindConvertMethod);
        }

        private static MethodInfo FindConvertMethod(Type type)
        {
            Func<MethodInfo, bool> parsePredicate = method =>
            {
                if (method.Name == "Parse")
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                }
                return false;
            };

            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsEnum ? _enumParseMethod : typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(parsePredicate);
        }

        public override int Match(ConverterMatchContext context)
        {
            if (context.SourceType == typeof(string))
            {
                var targetType = context.TargetType;
                if (targetType.IsNullable() && GetConvertMethod(targetType.GetTypeInfo().GetGenericArguments()[0]) != null) return 1;

                if (GetConvertMethod(context.TargetType) != null) return 0;
            }
            return -1;
        }

        public override void Compile(ModuleBuilder builder)
        {
        }

        public override void Emit(Type sourceType, Type targetType, CompilationContext context)
        {
            var reflectingTargetType = targetType.GetTypeInfo();
            var target = context.DeclareLocal(targetType);
            var local = context.DeclareLocal(sourceType);
            context.Emit(OpCodes.Stloc, local);

            if (targetType.IsNullable())
            {
                var labelEnd = context.DefineLabel();
                var labelFirst = context.DefineLabel();
                // if(source == null)
                context.Emit(OpCodes.Ldloc, local);
                context.Emit(OpCodes.Brtrue, labelFirst);

                // target = null;
                context.Emit(OpCodes.Ldloca, target);
                context.Emit(OpCodes.Initobj, targetType);

                // goto end;
                context.Emit(OpCodes.Br, labelEnd);
                context.MakeLabel(labelFirst);

                // if(string.IsNullOrWhiteSpace(source))
                var labelSecond = context.DefineLabel();
                context.Emit(OpCodes.Ldloc, local);
                context.EmitCall(_checkEmptyMethod);
                context.Emit(OpCodes.Brfalse, labelSecond);

                // target = new Nullable<T>(default(T));
                context.EmitDefault(reflectingTargetType.GetGenericArguments()[0]);
                context.Emit(OpCodes.Newobj, reflectingTargetType.GetConstructors()[0]);
                context.Emit(OpCodes.Stloc, target);

                // goto end;
                context.Emit(OpCodes.Br, labelEnd);
                context.MakeLabel(labelSecond);

                // target = new Nullable<$EnumType$>(($EnumType$)Enum.Parse(typeof($EnumType$),source));
                // or
                // target = new Nullable<$TargetType$>($TargetType$.Parse(source));
                var underlingType = reflectingTargetType.GetGenericArguments()[0];
                if (underlingType.GetTypeInfo().IsEnum)
                {
                    context.EmitTypeOf(underlingType);
                }
                context.Emit(OpCodes.Ldloc, local);
                context.EmitCall(_stringTrimMethod);
                context.EmitCall(GetConvertMethod(underlingType));
                context.EmitCast(underlingType);
                context.Emit(OpCodes.Newobj, reflectingTargetType.GetConstructors()[0]);
                context.Emit(OpCodes.Stloc, target);

                context.MakeLabel(labelEnd);
                context.Emit(OpCodes.Ldloc, target);
            }
            else
            {
                // if(!string.IsNullOrWhiteSpace(source))
                var label = context.DefineLabel();
                context.Emit(OpCodes.Ldloc, local);
                context.EmitCall(_checkEmptyMethod);
                context.Emit(OpCodes.Brtrue, label);

                // target = ($EnumType$)Enum.Parse(typeof($EnumType$),source);
                // or
                // target = $TargetType$.Parse(source);
                if (reflectingTargetType.IsEnum)
                {
                    context.EmitTypeOf(targetType);
                }
                context.Emit(OpCodes.Ldloc, local);
                context.EmitCall(_stringTrimMethod);
                context.EmitCall(GetConvertMethod(targetType));
                context.EmitCast(targetType);
                context.Emit(OpCodes.Stloc, target);

                // goto end;
                var labelEnd = context.DefineLabel();
                context.Emit(OpCodes.Br, labelEnd);

                // target = default(T);
                context.MakeLabel(label);
                context.EmitDefault(targetType);
                context.Emit(OpCodes.Stloc, target);

                context.MakeLabel(labelEnd);
                context.Emit(OpCodes.Ldloc, target);
            }
        }
    }
}
