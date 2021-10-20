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

                    if (nullableAttribute is not null)
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

        internal static FieldInfo? GetBackingField(this PropertyInfo property)
        {
            if (!property.CanRead || !property.GetGetMethod(nonPublic: true)!.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            var backingField = property!.DeclaringType!.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            backingField ??= property!.DeclaringType.GetField($"_{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}", BindingFlags.Instance | BindingFlags.NonPublic);

            backingField ??= property!.DeclaringType.GetField($"{char.ToLowerInvariant(property.Name[0])}{property.Name.Substring(1)}", BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingField == null)
                return null;

            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;

            return backingField;
        }
    }
}
