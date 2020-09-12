using System;
using System.Reflection;

namespace Venflow
{
    internal static class TypeExtensions
    {
        internal static PropertyInfo? FindProperty(this Type type, string propertyName, Type genericInterfaceType)
        {
            if (type.IsInterface)
            {
                return genericInterfaceType.MakeGenericType(type.GetGenericArguments()[0]).GetProperty(propertyName);
            }
            else
            {
                return type.GetProperty(propertyName);
            }
        }
    }
}
