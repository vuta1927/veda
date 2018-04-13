using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;

namespace VDS.Helpers
{
    #region ObjectActivator Delegates

    /// <summary>
    /// Defines an activator used to create an instance of a type.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>A new instance of the specified type.</returns>
    public delegate T ObjectActivator<out T>(params object[] args);

    /// <summary>
    /// Defines an activator used to create an instance of a type.
    /// </summary>
    /// <param name="args">The constructor arguments.</param>
    /// <returns>A new instance of the type.</returns>
    public delegate object ObjectActivator(params object[] args); 

    #endregion


    /// <summary>
    /// Provides methods for creating instances of objects.
    /// </summary>
    public static class ObjectFactory
    {
        #region Fields
        // ToDo-Low [08261014]: Alter use of unmanaged static cache to universal cache mechanism
        private static readonly IList<ObjectFactoryBinding> Bindings = new List<ObjectFactoryBinding>();
        private static readonly object BindingsLock = new object();
        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to be created by the activator.</param>
        /// <param name="args">The arguments to pass to the constructor.</param>
        /// <returns>A new instance of the specified type.</returns>
        public static object CreateInstance(Type type, params object[] args)
        {
            Throw.IfArgumentNull(type, "type");
            var activator = GetActivator(type, GetArgumentTypes(args));
            Throw.IfNull(activator)
                 .A<InvalidOperationException>("No constructor can be found for type '{0}' which accepts arguments: [{1}]."
                                               .FormatWith(type.FullName, string.Join(", ", args.Select(a => a.GetType().FullName))));
            return activator(args);
        }

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of instance to create.</typeparam>
        /// <param name="args">The arguments to pass to the constructor.</param>
        /// <returns>A new instance of the specified type.</returns>
        public static T CreateInstance<T>(params object[] args)
        {
            var activator = GetActivator<T>(GetArgumentTypes(args));
            Throw.IfNull(activator)
                 .A<InvalidOperationException>("No constructor can be found for type '{0}' which accepts arguments: [{1}]."
                                               .FormatWith(typeof(T).FullName, string.Join(", ", args.Select(a => a.GetType().FullName))));
            return activator(args);
        }

        /// <summary>
        /// Gets an object activator for the specified type and arguments.
        /// </summary>
        /// <param name="type">The type to be created by the activator.</param>
        /// <param name="argumentTypes">The types of arguments to pass to the constructor.</param>
        /// <returns>An instance of the specified activator, if faound, otherwise null</returns>
        public static ObjectActivator GetActivator(Type type, params Type[] argumentTypes)
        {
            Throw.IfArgumentNull(type, "type");
            argumentTypes = argumentTypes ?? new Type[0];

            lock (BindingsLock)
            {
                var binding = Bindings.SingleOrDefault(b => b.ObjectType == type && b.ConstructorArgumentTypes.SequenceEqual(argumentTypes));
                if (binding != null)
                    return (ObjectActivator) binding.Delegate;

                var activator = CreateActivator(type, argumentTypes);
                Bindings.Add(new ObjectFactoryBinding(type, argumentTypes, activator));
                return activator;
            }
        }

        /// <summary>
        /// Gets an object activator for the specified type and arguments.
        /// </summary>
        /// <typeparam name="T">The type to be created by the activator.</typeparam>
        /// <param name="argumentTypes">The types of arguments to pass to the constructor.</param>
        /// <returns>An instance of the specified activator, if faound, otherwise null</returns>
        public static ObjectActivator<T> GetActivator<T>(params Type[] argumentTypes)
        {
            argumentTypes = argumentTypes ?? new Type[0];
            var type = typeof(T);

            lock (BindingsLock)
            {
                var binding = Bindings.SingleOrDefault(b => b.ObjectType == type && b.ConstructorArgumentTypes.SequenceEqual(argumentTypes));
                if (binding != null)
                    return (ObjectActivator<T>)binding.Delegate;

                var activator = CreateActivator<T>(argumentTypes);
                Bindings.Add(new ObjectFactoryBinding(type, argumentTypes, activator));
                return activator;
            }
        }

        /// <summary>
        /// Confirms the existence of an object activator for the specified type and arguments.
        /// </summary>
        /// <param name="type">The type to be created by the activator.</param>
        /// <param name="argumentTypes">The types of arguments to pass to the constructor.</param>
        /// <returns>True if exists, otherwise false.</returns>
        public static bool ConstructorExists(Type type, params Type[] argumentTypes)
        {
            return GetActivator(type, argumentTypes) != null;
        }

        /// <summary>
        /// Confirms the existence of an object activator for the specified type and arguments.
        /// </summary>
        /// <typeparam name="T">The type to be created by the activator.</typeparam>
        /// <param name="argumentTypes">The types of arguments to pass to the constructor.</param>
        /// <returns>True if exists, otherwise false.</returns>
        public static bool ConstructorExists<T>(params Type[] argumentTypes)
        {
            return GetActivator(typeof(T), argumentTypes) != null;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Creates an object activator for the specified type and arguments.
        /// </summary>
        /// <param name="type">The type to be created by the activator.</param>
        /// <param name="arguments">The argument types to pass to the constructor.</param>
        /// <returns>An instance of the specified activator, if found, otherwise null</returns>
        private static ObjectActivator CreateActivator(Type type, Type[] arguments)
        {
            var constructor = type.GetConstructor(arguments);
            if (constructor == null) return null;
            var expression = CreateExpression(constructor, arguments);
            var activator = (ObjectActivator)expression.Compile();
            return activator;
        }

        /// <summary>
        /// Creates an object activator for the specified type and arguments.
        /// </summary>
        /// <typeparam name="T">The type to be created by the activator.</typeparam>
        /// <param name="arguments">The argument types to pass to the constructor.</param>
        /// <returns>An instance of the specified activator, if found, otherwise null</returns>
        private static ObjectActivator<T> CreateActivator<T>(Type[] arguments)
        {
            Type type = typeof(T);
            var constructor = type.GetConstructor(arguments);
            if (constructor == null) return null;
            var expression = CreateExpression<T>(constructor, arguments);
            var activator = (ObjectActivator<T>)expression.Compile();
            return activator;
        }

        /// <summary>
        /// Creates a lambda expression to create the activator instance.
        /// </summary>
        /// <param name="constructor">The constructor used to create the instance.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <returns>A lambda expression to create the activator instance.</returns>
        private static LambdaExpression CreateExpression(ConstructorInfo constructor, Type[] argTypes)
        {
            var param = Expression.Parameter(typeof(object[]), "args");
            var args = new Expression[argTypes.Length];

            for (int i = 0; i < argTypes.Length; i++)
            {
                var index = Expression.Constant(i);
                Type paramType = argTypes[i];

                var accessor = Expression.ArrayIndex(param, index);
                var cast = Expression.Convert(accessor, paramType);

                args[i] = cast;
            }

            var @new = Expression.New(constructor, args);
            var lambda = Expression.Lambda(typeof(ObjectActivator), @new, param);

            return lambda;
        }

        /// <summary>
        /// Creates a lambda expression to create the activator instance.
        /// </summary>
        /// <typeparam name="T">The type to be created by the activator.</typeparam>
        /// <param name="constructor">The constructor used to create the instance.</param>
        /// <param name="argTypes">The argument types.</param>
        /// <returns>A lambda expression to create the activator instance.</returns>
        private static LambdaExpression CreateExpression<T>(ConstructorInfo constructor, Type[] argTypes)
        {
            var param = Expression.Parameter(typeof(object[]), "args");
            var args = new Expression[argTypes.Length];

            for (int i = 0; i < argTypes.Length; i++)
            {
                var index = Expression.Constant(i);
                Type paramType = argTypes[i];

                var accessor = Expression.ArrayIndex(param, index);
                var cast = Expression.Convert(accessor, paramType);

                args[i] = cast;
            }

            var @new = Expression.New(constructor, args);
            var lambda = Expression.Lambda(typeof(ObjectActivator<T>), @new, param);

            return lambda;
        }

        /// <summary>
        /// Gets an array of types for the specified arguments.
        /// </summary>
        /// <param name="args">The arguments to get types for.</param>
        /// <returns>An array of types.</returns>
        private static Type[] GetArgumentTypes(params object[] args)
        {
            if (args == null)
                return new Type[0];

            return args
                .Select(a => a.GetType())
                .ToArray();
        }
        #endregion


        /// <summary>
        /// Defines a binding between a type, the constructor used in its creation, and the delegate used to create it.
        /// </summary>
        private class ObjectFactoryBinding
        {
            #region Constructor
            /// <summary>
            /// Initialises a new instance of <see cref="ObjectFactoryBinding"/>
            /// </summary>
            /// <param name="objectType">The object type.</param>
            /// <param name="constructorArgumentTypes">The constructor argument types.</param>
            /// <param name="delegate">The delegate used to create it.</param>
            public ObjectFactoryBinding(Type objectType, Type[] constructorArgumentTypes, Delegate @delegate)
            {
                Throw.IfArgumentNull(objectType, "objectType");
                Throw.IfArgumentNull(constructorArgumentTypes, "constructorArgumentTypes");

                ObjectType = objectType;
                ConstructorArgumentTypes = constructorArgumentTypes;
                Delegate = @delegate;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the object type.
            /// </summary>
            public Type ObjectType { get; private set; }

            /// <summary>
            /// Gets the constructor argument types.
            /// </summary>
            public Type[] ConstructorArgumentTypes { get; private set; }

            /// <summary>
            /// Gets the delegate used to create it.
            /// </summary>
            public Delegate Delegate { get; private set; }
            #endregion
        }
    }
}