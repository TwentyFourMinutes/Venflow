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

        internal static MethodInfo? GetCastMethod(this Type type, Type sourceType, Type targetType)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

            for (var methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                var method = methods[methodIndex];

                if ((method.Name is not "op_Implicit" and not "op_Explicit") ||
                    method.ReturnType != targetType ||
                    method.GetParameters().Length != 1 ||
                    method.GetParameters()[0].ParameterType != sourceType.MakeByRefType())
                    continue;

                return method;
            }

            return null;
        }
    }
}
