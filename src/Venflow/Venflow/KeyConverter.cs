using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection.Emit;
using Venflow.Dynamic;

namespace Venflow
{
    internal class KeyConverter : TypeConverter
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> _typeConverters = new();
        private static readonly ConcurrentDictionary<Type, Delegate> _keyFactories = new();

        private readonly TypeConverter _underlyingConverter;

        public KeyConverter(Type keyType)
        {
            _underlyingConverter = _typeConverters.GetOrAdd(keyType, CreateTypeConverter);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => _underlyingConverter.CanConvertFrom(context, sourceType);
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => _underlyingConverter.CanConvertTo(context, destinationType);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => _underlyingConverter.ConvertFrom(context, culture, value);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            => _underlyingConverter.ConvertTo(context, culture, value, destinationType);

        internal static Func<TValue, object> GetOrCreateKeyFactory<TValue>(Type keyType)
            => (Func<TValue, object>)_keyFactories.GetOrAdd(keyType, CreateKeyFactory<TValue>);

        private static TypeConverter CreateTypeConverter(Type keyType)
        {
            var keyInterface = keyType.GetInterface("Venflow.IKey`2");

            if (keyInterface is null)
                throw new InvalidOperationException($"Cannot create converter for type '{keyType}'.");

            var parameters = keyInterface.GetGenericArguments();

            return (TypeConverter)Activator.CreateInstance(typeof(KeyConverter<,>).MakeGenericType(parameters[0], parameters[1]), keyType)!;
        }

        private static Func<TValue, object> CreateKeyFactory<TValue>(Type keyType)
        {
            if (!typeof(IKey).IsAssignableFrom(keyType))
                throw new ArgumentException($"Type '{keyType}' is not a key type.", nameof(keyType));

            var ctor = keyType.GetConstructor(new[] { typeof(TValue) });

            if (ctor is null)
                throw new ArgumentException($"Type '{keyType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'.", nameof(keyType));

            var method = TypeFactory.GetDynamicMethod(keyType.Name + "Instantiater", typeof(object), new[] { typeof(TValue) });
            var ilGenerator = method.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, ctor);
            ilGenerator.Emit(OpCodes.Box, keyType);
            ilGenerator.Emit(OpCodes.Ret);

            return (Func<TValue, object>)method.CreateDelegate(typeof(Func<TValue, object>));
        }
    }

    internal class KeyConverter<TEntity, TKeyValue> : TypeConverter
        where TKeyValue : struct, IEquatable<TKeyValue>
    {
        private static readonly TypeConverter _keyConverter = GetKeyConverter();

        private readonly Type _type;
        internal KeyConverter(Type type)
        {
            _type = type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ||
                   sourceType == typeof(TKeyValue) ||
                   base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(TKeyValue) ||
                   base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object sourceValue)
        {
            if (sourceValue is string value)
            {
                sourceValue = _keyConverter.ConvertFrom(value);
            }

            if (sourceValue is TKeyValue keyValue)
            {
                return KeyConverter.GetOrCreateKeyFactory<TKeyValue>(_type).Invoke(keyValue);
            }
            else
            {
                return base.ConvertFrom(context, culture, sourceValue);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is not IKey<TEntity, TKeyValue> { Value: var keyValue })
                throw new ArgumentNullException(nameof(value));

            if (destinationType == typeof(string))
            {
                return keyValue.ToString()!;
            }
            else if (destinationType == typeof(TKeyValue))
            {
                return keyValue;
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        private static TypeConverter GetKeyConverter()
        {
            var converter = TypeDescriptor.GetConverter(typeof(TKeyValue));

            if (!converter.CanConvertFrom(typeof(string)))
                throw new InvalidOperationException($"No TypeConverter for type '{typeof(TKeyValue)}' could be found.");

            return converter;
        }
    }
}
