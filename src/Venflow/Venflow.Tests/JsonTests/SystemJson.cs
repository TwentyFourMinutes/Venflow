using System;
using System.Text.Json;
using Venflow.Tests.Keys;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.JsonTests
{
    public class SystemJson : TestBase
    {
        [Fact]
        public void TwoWayParsing()
        {
            var uncommonType = new UncommonType
            {
                GuidKey = Guid.NewGuid(),
                NGuidKey = Guid.NewGuid()
            };

            var value = JsonSerializer.Serialize(uncommonType);

            var parsedUncommonType = JsonSerializer.Deserialize<UncommonType>(value);

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

            var value = JsonSerializer.Serialize(uncommonType);

            var parsedUncommonType = JsonSerializer.Deserialize<UncommonType>(value);

            Assert.NotNull(parsedUncommonType);

            Assert.Equal(uncommonType.NGuidKey, parsedUncommonType.NGuidKey);
        }

        [Fact]
        public void TwoWayParsingOfKey()
        {
            var entity = new Entity();

            var value = JsonSerializer.Serialize(entity);

            var parsedEntity = JsonSerializer.Deserialize<Entity>(value);

            Assert.NotNull(parsedEntity);

            Assert.Equal(entity.Key, parsedEntity.Key);
        }

        private class Entity
        {
            public Key<Entity> Key { get; set; } = Guid.NewGuid();
        }
    }
}