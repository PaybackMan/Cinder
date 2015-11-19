using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public static class TypeSystem
    {
        public static Type GetElementType(Type seqType)
        {
            if (seqType.IsArray)
            {
                return seqType.GetElementType();
            }

            Type ienum = FindIEnumerable(seqType);
            if (ienum == null)
            {
                return seqType;
            }
            return ienum.GetTypeInfo().GenericTypeArguments[0];
        }
        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;
            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            if (seqType.GetTypeInfo().IsGenericType)
            {
                foreach (Type arg in seqType.GetTypeInfo().GenericTypeArguments)
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.GetTypeInfo().IsAssignableFrom(seqType.GetTypeInfo()))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetTypeInfo().ImplementedInterfaces.ToArray();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null) return ienum;
                }
            }
            if (seqType.GetTypeInfo().BaseType != null && seqType.GetTypeInfo().BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.GetTypeInfo().BaseType);
            }
            return null;
        }

        public static bool IsTypeOfOrSubclassOf(this Type type, Type targetType)
        {
            return type.Equals(targetType) || type.GetTypeInfo().IsSubclassOf(targetType);
        }

        /// <summary>
        /// Returns true if the current type implements the provided concrete interface type, or generic interface type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Implements<T>(this Type type)
        {
            return Implements(type, typeof(T));
        }

        /// <summary>
        /// Returns true if the current type implements the provided concrete interface type, or generic interface type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool Implements(this Type type, Type interfaceType)
        {
            return type == interfaceType 
                || (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == interfaceType)
                || type.GetTypeInfo().ImplementedInterfaces.Any(i => 
                    i.Equals(interfaceType) || (i.IsConstructedGenericType && i.GetGenericTypeDefinition().Equals(interfaceType)));
        }
    }
}
