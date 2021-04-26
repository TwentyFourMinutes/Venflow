using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace Venflow.NewtonsoftJson
{
    /// <summary>
    /// A <see cref="JsonConverter"/> to parse <see cref="IKey{TEntity, TKey}"/> instances.
    /// </summary>
    public class NewtonsoftJsonKeyConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> _jsonConverters = new();

        public override bool CanConvert(Type objectType)
        {
            return typeof(IKey).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var converter = GetConverter(objectType);

            return converter.ReadJson(reader, objectType, existingValue, serializer);
        }

        public override void WriteJson(JsonWriter writer, object value,JsonSerializer serializer)
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

        private static JsonConverter GetConverter(Type keyType)
        {
            return _jsonConverters.GetOrAdd(keyType, CreateConverter);
        }

        private static JsonConverter CreateConverter(Type keyType)
        {
            var keyInterface = keyType.GetInterface("Venflow.IKey`2");

            if (keyInterface is null)
                throw new InvalidOperationException($"Cannot create converter for type '{keyType}'.");

            var parameters = keyInterface.GetGenericArguments();

            return (JsonConverter)Activator.CreateInstance(typeof(NewtonsoftJsonKeyConverter<,,>).MakeGenericType(keyType, parameters[0], parameters[1]));
        }
    }

    internal class NewtonsoftJsonKeyConverter<TKey, TEntity, TKeyValue> : JsonConverter<TKey>
        where TKey : struct
        where TKeyValue : struct
    {
        public override TKey ReadJson(JsonReader reader, Type objectType, TKey existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType is JsonToken.Null)
                return default;

            var value = serializer.Deserialize<TKeyValue>(reader);
            var factory = KeyConverter.GetOrCreateKeyFactory<TKeyValue>(objectType);

            return (TKey)factory(value);
        }

        public override void WriteJson(JsonWriter writer, TKey value, JsonSerializer serializer)
        {
            writer.WriteValue(((IKey<TEntity, TKeyValue>)value).Value);
        }
    }
}
