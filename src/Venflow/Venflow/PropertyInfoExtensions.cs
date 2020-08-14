using System.Reflection;
using System.Runtime.CompilerServices;

namespace Venflow
{
    internal static class PropertyInfoExtensions
    {
        internal static bool IsNullableReferenceType(this PropertyInfo property, bool isClassInNullableContext, bool defaultPropNullability)
        {
            if (property.PropertyType.IsClass)
            {
                if (isClassInNullableContext)
                {
                    var nullableAttribute = property.GetCustomAttribute<NullableAttribute>();

                    if (nullableAttribute is { })
                    {
                        // Flag == 1 prop is not null-able if not otherwise specified. Flag == 2 reversed.
                        return nullableAttribute.NullableFlags[0] == 2;
                    }
                    else
                    {
                        return defaultPropNullability;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
