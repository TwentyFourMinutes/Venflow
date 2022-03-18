using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Reflow
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class KeyConverterFactory : TypeConverter
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> _converters = new();

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType.GetInterfaces().Any(x => x == typeof(IKey));
        }

        public override object? ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value
        )
        {
            var type = value.GetType();

            if (!CanConvertFrom(type))
            {
                return null!;
            }

            return _converters
                .GetOrAdd(type.GetGenericTypeDefinition(), GetConverter)
                .ConvertFrom(context, culture, value);
        }

        public override object? ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType
        )
        {
            return _converters
                .GetOrAdd(destinationType.GetGenericTypeDefinition(), GetConverter)
                .ConvertTo(context, culture, value, destinationType);
        }

        private static TypeConverter GetConverter(Type keyType)
        {
            var converterType = keyType.GetNestedType(
                "KeyConverter",
                BindingFlags.NonPublic
            )!.MakeGenericType(keyType.GenericTypeArguments[0])!;

            return (TypeConverter)Activator.CreateInstance(converterType)!;
        }
    }
}
