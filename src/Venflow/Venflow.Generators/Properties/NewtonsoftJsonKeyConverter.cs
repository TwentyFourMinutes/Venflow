using System;
using System.Collections.Concurrent;

namespace Venflow.Json
{
    /// <summary>
    /// A <see cref="Newtonsoft.Json.JsonConverter"/> to parse <see cref="Venflow.IKey{TEntity, TKey}"/> instances.
    /// </summary>
    public class NewtonsoftJsonKeyConverter : Newtonsoft.Json.JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, Newtonsoft.Json.JsonConverter> _jsonConverters = new();

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Venflow.IKey).IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var converter = GetConverter(objectType);

            return converter.ReadJson(reader, objectType, existingValue, serializer);
        }

        /// <inheritdoc/>
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                var converter = GetConverter(value.GetType());
                converter.WriteJson(writer, value, serializer);
            }
        }

        private static Newtonsoft.Json.JsonConverter GetConverter(Type keyType)
        {
            return _jsonConverters.GetOrAdd(keyType, CreateConverter);
        }

        private static Newtonsoft.Json.JsonConverter CreateConverter(Type keyType)
        {
            var keyInterface = keyType.GetInterface("Venflow.IKey`2");

            if (keyInterface is null)
                throw new InvalidOperationException($"Cannot create converter for type '{keyType}'.");

            var parameters = keyInterface.GetGenericArguments();

            return (Newtonsoft.Json.JsonConverter)Activator.CreateInstance(typeof(NewtonsoftJsonKeyConverter<,,>).MakeGenericType(keyType, parameters[0], parameters[1]));
        }
    }

    internal class NewtonsoftJsonKeyConverter<TKey, TEntity, TKeyValue> : Newtonsoft.Json.JsonConverter<TKey>
        where TKey : struct
        where TKeyValue : struct
    {
        public override TKey ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, TKey existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType is Newtonsoft.Json.JsonToken.Null)
                return default;

            var value = serializer.Deserialize<TKeyValue>(reader);
            var factory = KeyConverter.GetOrCreateKeyFactory<TKeyValue>(objectType);

            return (TKey)factory(value);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, TKey value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteValue(((Venflow.IKey<TEntity, TKeyValue>)value).Value);
        }
    }
}
