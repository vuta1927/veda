using System;
using System.Reflection;

namespace VDS.Dependency.Registration
{
    /// <summary>
    ///   Factory for creating <see cref = "ComponentRegistration" /> objects. Use static methods on the class to fluently build registration.
    /// </summary>
    public static class Component
    {
        /// <summary>
        ///   Helper method for filtering components based on presence of an Attribute.
        /// </summary>
        /// <typeparam name = "TAttribute"></typeparam>
        /// <param name = "type"></param>
        /// <returns></returns>
        /// <example>
        ///   container.Register(
        ///   Classes.FromThisAssembly()
        ///   .Where(Component.HasAttribute&lt;UserAttribute&gt;) );
        /// </example>
        public static bool HasAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            return type.GetTypeInfo().IsDefined(typeof(TAttribute));
        }

        /// <summary>
        ///   Helper method for filtering components based on presence of an Attribute and value of predicate on that attribute.
        /// </summary>
        /// <typeparam name = "TAttribute"></typeparam>
        /// <param name = "filter"></param>
        /// <returns></returns>
        /// <example>
        ///   container.Register(
        ///   Classes.FromThisAssembly()
        ///   .Where(Component.HasAttribute&lt;UserAttribute&gt;(u => u.SomeFlag)) );
        /// </example>
        public static Predicate<Type> HasAttribute<TAttribute>(Predicate<TAttribute> filter) where TAttribute : Attribute
        {
            return type => HasAttribute<TAttribute>(type) &&
                           filter((TAttribute)type.GetTypeInfo().GetCustomAttribute(typeof(TAttribute)));
        }

        /// <summary>
        ///   Creates a predicate to check if a component is in a namespace.
        /// </summary>
        /// <param name = "namespace">The namespace.</param>
        /// <returns>true if the component type is in the namespace.</returns>
        public static Predicate<Type> IsInNamespace(string @namespace)
        {
            return IsInNamespace(@namespace, false);
        }

        /// <summary>
        ///   Creates a predicate to check if a component is in a namespace.
        /// </summary>
        /// <param name = "namespace">The namespace.</param>
        /// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
        /// <returns>true if the component type is in the namespace.</returns>
        public static Predicate<Type> IsInNamespace(string @namespace, bool includeSubnamespaces)
        {
            if (includeSubnamespaces)
            {
                return type => type.Namespace == @namespace ||
                               type.Namespace != null &&
                               type.Namespace.StartsWith(@namespace + ".");
            }

            return type => type.Namespace == @namespace;
        }

        /// <summary>
        ///   Creates a predicate to check if a component shares a namespace with another.
        /// </summary>
        /// <param name = "type">The component type to test namespace against.</param>
        /// <returns>true if the component is in the same namespace.</returns>
        public static Predicate<Type> IsInSameNamespaceAs(Type type)
        {
            return IsInNamespace(type.Namespace);
        }

        /// <summary>
        ///   Creates a predicate to check if a component shares a namespace with another.
        /// </summary>
        /// <param name = "type">The component type to test namespace against.</param>
        /// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
        /// <returns>true if the component is in the same namespace.</returns>
        public static Predicate<Type> IsInSameNamespaceAs(Type type, bool includeSubnamespaces)
        {
            return IsInNamespace(type.Namespace, includeSubnamespaces);
        }

        /// <summary>
        ///   Creates a predicate to check if a component shares a namespace with another.
        /// </summary>
        /// <typeparam name = "T">The component type to test namespace against.</typeparam>
        /// <returns>true if the component is in the same namespace.</returns>
        public static Predicate<Type> IsInSameNamespaceAs<T>()
        {
            return IsInSameNamespaceAs(typeof(T));
        }

        /// <summary>
        ///   Creates a predicate to check if a component shares a namespace with another.
        /// </summary>
        /// <typeparam name = "T">The component type to test namespace against.</typeparam>
        /// <param name = "includeSubnamespaces">If set to true, will also include types from subnamespaces.</param>
        /// <returns>true if the component is in the same namespace.</returns>
        public static Predicate<Type> IsInSameNamespaceAs<T>(bool includeSubnamespaces)
        {
            return IsInSameNamespaceAs(typeof(T), includeSubnamespaces);
        }
    }
}