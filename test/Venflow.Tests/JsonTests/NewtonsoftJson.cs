using System;
using Newtonsoft.Json;
using Venflow.NewtonsoftJson;
using Venflow.Tests.Keys;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.JsonTests
{
    public class NewtonsoftJson : TestBase
    {
        [Fact]
        public void TwoWayParsing()
        {
            var uncommonType = new UncommonType
            {
                GuidKey = Guid.NewGuid(),
                NGuidKey = Guid.NewGuid()
            };

            var settings = new JsonSerializerSettings { Converters = new[] { new NewtonsoftJsonKeyConverter() } };

            var value = JsonConvert.SerializeObject(uncommonType, settings);

            var parsedUncommonType = JsonConvert.DeserializeObject<UncommonType>(value, settings);

            Assert.NotNull(parsedUncommonType);

            Assert.Equal(uncommonType.GuidKey, parsedUncommonType.GuidKey);
            Assert.Equal(uncommonType.NGuidKey, parsedUncommonType.NGuidKey);
        }

        [Fact]
        public void TwoWayParsingOfNull()
        {
            var uncommonType = new UncommonType
            {
                NGuidKey = null
            };

            var settings = new JsonSerializerSettings { Converters = new[] { new NewtonsoftJsonKeyConverter() } };

            var value = JsonConvert.SerializeObject(uncommonType, settings);

            var parsedUncommonType = JsonConvert.DeserializeObject<UncommonType>(value, settings);

            Assert.NotNull(parsedUncommonType);

            Assert.Equal(uncommonType.NGuidKey, parsedUncommonType.NGuidKey);
        }

        [Fact]
        public void TwoWayParsingOfKey()
        {
            var entity = new Entity();

            var settings = new JsonSerializerSettings { Converters = new[] { new NewtonsoftJsonKeyConverter() } };

            var value = JsonConvert.SerializeObject(entity, settings);

            var parsedEntity = JsonConvert.DeserializeObject<Entity>(value, settings);

            Assert.NotNull(parsedEntity);

            Assert.Equal(entity.Key, parsedEntity.Key);
        }

        private class Entity
        {
            public Key<Entity> Key { get; set; } = Guid.NewGuid();
        }
    }
}
