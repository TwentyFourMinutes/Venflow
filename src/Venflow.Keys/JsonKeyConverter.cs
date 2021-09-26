using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Venflow.Json
{
    /// <summary>
    /// A <see cref="JsonConverter"/> to parse <see cref="IKey{TEntity, TKey}"/> instances.
    /// </summary>
    public class JsonKeyConverterFactory : JsonConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> _jsonConverters = new();

        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
            => typeof(IKey).IsAssignableFrom(typeToConvert);

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => _jsonConverters.GetOrAdd(typeToConvert, CreateJsonConverter);

        private JsonConverter CreateJsonConverter(Type typeToConvert)
        {
            var keyInterface = typeToConvert.GetInterface("Venflow.IKey`2");

            if (keyInterface is null)
                throw new InvalidOperationException($"Cannot create converter for type '{typeToConvert}'.");

            var parameters = keyInterface.GetGenericArguments();

            return (JsonConverter)Activator.CreateInstance(typeof(JsonKeyConverter<,,>).MakeGenericType(typeToConvert, parameters[0], parameters[1]));
        }
    }

    internal class JsonKeyConverter<TKey, TEntity, TKeyValue> : JsonConverter<TKey>
        where TKey : struct, IKey<TEntity, TKeyValue>
        where TKeyValue : struct
    {
        public override TKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.Null)
                return default;

            var value = JsonSerializer.Deserialize<TKeyValue>(ref reader, options);

            var factory = KeyConverter.GetOrCreateKeyFactory<TKey, TKeyValue>(typeToConvert);

            return factory(value);
        }

        public override void Write(Utf8JsonWriter writer, TKey value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, ((IKey<TEntity, TKeyValue>)value).Value, options);
        }
    }
}
