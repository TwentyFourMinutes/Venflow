using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Venflow
{
    /// <summary>
    /// Provides a way of converting <see cref="Key{TEntity, TKey}"/> to strings and vice versa.
    /// </summary>
    public class KeyConverter : TypeConverter
    {
        private static int _typeNumberIdentifier = 0;

        private static readonly ConcurrentDictionary<Type, TypeConverter> _typeConverters = new(Environment.ProcessorCount, 10);
        private static readonly ConcurrentDictionary<Type, Delegate> _keyFactories = new(Environment.ProcessorCount, 10);
        private static readonly ConcurrentDictionary<Type, Delegate> _objectKeyFactories = new(Environment.ProcessorCount, 0);

        private readonly TypeConverter _underlyingConverter;


        /// <summary>
        /// Creates a new instance of a <see cref="KeyConverter"/> with the given key type.
        /// </summary>
        /// <param name="keyType">The type of the key to which the <see cref="KeyConverter"/> should bind to.</param>
        public KeyConverter(Type keyType)
        {
            _underlyingConverter = _typeConverters.GetOrAdd(keyType, CreateTypeConverter);
        }

        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => _underlyingConverter.CanConvertFrom(context, sourceType);

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => _underlyingConverter.CanConvertTo(context, destinationType);

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            => _underlyingConverter.ConvertFrom(context, culture, value);

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            => _underlyingConverter.ConvertTo(context, culture, value, destinationType);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<TValue, object> GetOrCreateKeyFactory<TValue>(Type keyType)
            => (Func<TValue, object>)_objectKeyFactories.GetOrAdd(keyType, CreateKeyFactory<object, TValue>);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<TValue, TKeyType> GetOrCreateKeyFactory<TKeyType, TValue>(Type keyType) where TKeyType : struct, IKey
            => (Func<TValue, TKeyType>)_keyFactories.GetOrAdd(keyType, CreateKeyFactory<TKeyType, TValue>);

        private static TypeConverter CreateTypeConverter(Type keyType)
        {
            var keyInterface = keyType.GetInterface("Venflow.IKey`2");

            if (keyInterface is null)
                throw new InvalidOperationException($"Cannot create converter for type '{keyType}'.");

            var parameters = keyInterface.GetGenericArguments();

            var ctor = typeof(KeyConverter<,>).MakeGenericType(parameters[0], parameters[1]).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Type) }, null);

            return (TypeConverter)ctor.Invoke(new[] { keyType });
        }

        private static Func<TValue, TKeyType> CreateKeyFactory<TKeyType, TValue>(Type keyType)
        {
            if (!typeof(IKey).IsAssignableFrom(keyType))
                throw new ArgumentException($"Type '{keyType}' is not a key type.", nameof(keyType));

            var ctor = keyType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(TValue) }, null);

            if (ctor is null)
                throw new ArgumentException($"Type '{keyType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'.", nameof(keyType));

            var method = new DynamicMethod(keyType.Name + "Instantiater" + "_" + Interlocked.Increment(ref _typeNumberIdentifier), typeof(TKeyType), new[] { typeof(TValue) }, true);
            var ilGenerator = method.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, ctor);

            if (typeof(TKeyType) == typeof(object))
            {
                ilGenerator.Emit(OpCodes.Box, keyType);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return (Func<TValue, TKeyType>)method.CreateDelegate(typeof(Func<TValue, TKeyType>));
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
