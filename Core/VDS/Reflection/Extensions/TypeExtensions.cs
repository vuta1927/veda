using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Exception;

namespace VDS.Reflection.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsPrimitiveExtendedIncludingNullable(this Type type, bool includeEnums = false)
        {
            if (IsPrimitiveExtended(type, includeEnums))
            {
                return true;
            }

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return IsPrimitiveExtended(type.GenericTypeArguments[0], includeEnums);
            }

            return false;
        }

        private static bool IsPrimitiveExtended(Type type, bool includeEnums)
        {
            if (type.GetTypeInfo().IsPrimitive)
            {
                return true;
            }

            if (includeEnums && type.GetTypeInfo().IsEnum)
            {
                return true;
            }

            return type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }

        public static bool IsComplex(this Type type)
        {
            Throw.IfArgumentNull(type, nameof(type));
            return !(TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string)));
        }

        public static bool AllowsNullValue(this Type type)
        {
            Throw.IfArgumentNull(type, nameof(type));
            var isNullabeValueType = Nullable.GetUnderlyingType(type) != null;
            return (!type.IsValueType || isNullabeValueType);
        }

        public static object GetDefault(this Type type)
        {
            Throw.IfArgumentNull(type, nameof(type));
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        #region GetPropertyInfo

        /// <summary>
        /// Searches recursively through an object tree for property information.
        /// </summary>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <returns>PropertyInfo of the specified property if it can find it otherwise returns null.</returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            return GetPropertyInfo(type, propertyName, null, PropertyAccessRequired.Any);

        }

        /// <summary>
        /// Searches recursively through an object tree for property information.
        /// </summary>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="access">The required property access.</param>
        /// <returns>PropertyInfo of the specified property if it can find it otherwise returns null.</returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName, PropertyAccessRequired access)
        {
            return GetPropertyInfo(type, propertyName, null, access);
        }

        /// <summary>
        /// Searches recursively through an object tree for property information.
        /// </summary>
        /// <param name="type">The type of the object to get the property from.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="propertyType">The type of the property to search for.</param>
        /// <returns>PropertyInfo of the specified property if it can find it otherwise returns null.</returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName, Type propertyType)
        {
            return GetPropertyInfo(type, propertyName, propertyType, PropertyAccessRequired.Any);
        }

        /// <summary>
        /// Searches recursively through an object tree for property information.
        /// </summary>
        /// <param name="type">The type of the object to get the property from.</param>
        /// <param name="propertyName">The name of the property to search for.</param>
        /// <param name="propertyType">The type of the property to search for.</param>
        /// <param name="access">The required property access.</param>
        /// <returns>PropertyInfo of the specified property if it can find it otherwise returns null.</returns>
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName, Type propertyType, PropertyAccessRequired access)
        {
            Throw.IfArgumentNullOrEmpty(propertyName, nameof(propertyName));
            var pi = (propertyType != null)
                                  ? type.GetProperty(propertyName,
                                                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static, null, propertyType, new Type[0], null)
                                  : type.GetProperty(propertyName,
                                                     BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
            var hasNoAccess = (pi != null && !HasRequiredAccess(pi, access));
            if (pi == null || hasNoAccess)
            {
                if (type.BaseType != null)
                {
                    pi = GetPropertyInfo(type.BaseType, propertyName, propertyType, access);
                }
            }
            return pi;
        }

        private static bool HasRequiredAccess(PropertyInfo pi, PropertyAccessRequired access)
        {
            Throw.IfArgumentNull(pi, nameof(pi));
            switch (access)
            {
                case PropertyAccessRequired.Any: return true;
                case PropertyAccessRequired.All: return pi.CanRead && pi.CanWrite;
                case PropertyAccessRequired.Get: return pi.CanRead;
                case PropertyAccessRequired.Set: return pi.CanWrite;
                default:
                    throw new ArgumentOutOfRangeException("access");
            }
        }
        #endregion

        #region GetFieldInfo

        /// <summary>
        /// Searches recursively through an object tree for field information.
        /// </summary>
        /// <param name="type">The type of the object to get the field from.</param>
        /// <param name="fieldName">The name of the field to search for.</param>
        /// <returns>FieldInfo of the specified field if it can find it otherwise returns null.</returns>
        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            return GetFieldInfo(type, fieldName, null);
        }

        /// <summary>
        /// Searches recursively through an object tree for field information.
        /// </summary>
        /// <param name="type">The type of the object to get the field from.</param>
        /// <param name="fieldName">The name of the field to search for.</param>
        /// <param name="fieldType">The type of the field to search for.</param>
        /// <returns>FieldInfo of the specified field if it can find it otherwise returns null.</returns>
        public static FieldInfo GetFieldInfo(this Type type, string fieldName, Type fieldType)
        {
            Throw.IfArgumentNullOrEmpty(fieldName, nameof(fieldName));
            FieldInfo fi = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
            if (fi == null || (fieldType != null && fi.FieldType != fieldType))
            {
                if (type.BaseType != null)
                {
                    fi = GetFieldInfo(type.BaseType, fieldName, fieldType);
                }
            }

            return fi;
        }

        #endregion


        public static bool HasDefaultConstructor(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var defaultConstructor = type.GetTypeInfo().DeclaredConstructors.Any(c => c.GetParameters().Length == 0);

            return typeInfo.IsValueType || defaultConstructor;
        }

        public static Assembly GetAssembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }
    }

    /// <summary>
    /// A selecton of enumerators.
    /// </summary>
    public enum PropertyAccessRequired
    {
        Any,
        All,
        Get,
        Set
    }
}