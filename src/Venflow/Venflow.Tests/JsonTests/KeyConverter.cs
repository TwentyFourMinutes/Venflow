using System;
using System.ComponentModel;
using Venflow.Tests.Keys;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.JsonTests
{
    public class KeyConverter : TestBase
    {
        [Fact]
        public void TwoWayParsing()
        {
            var uncommonType = new UncommonType
            {
                GuidKey = Guid.NewGuid(),
                NGuidKey = Guid.NewGuid()
            };

            var guidConverter = TypeDescriptor.GetConverter(uncommonType.GuidKey.GetType());
            var nullableGuidConverter = TypeDescriptor.GetConverter(uncommonType.NGuidKey.GetType());

            var guidValue = guidConverter.ConvertTo(uncommonType.GuidKey, typeof(Guid));
            var nullableGuidValue = nullableGuidConverter.ConvertTo(uncommonType.NGuidKey, typeof(Guid));

            Assert.Equal((Guid)uncommonType.GuidKey, guidValue);
            Assert.Equal((Guid)uncommonType.NGuidKey, nullableGuidValue);

            var boxedGuidValue = (Key<UncommonType, Guid>)guidConverter.ConvertFrom(guidValue);
            var boxedNullableGuidValue = (Key<UncommonType, Guid>)guidConverter.ConvertFrom(nullableGuidValue);

            Assert.Equal(uncommonType.GuidKey, boxedGuidValue);
            Assert.Equal(uncommonType.NGuidKey, boxedNullableGuidValue);
        }

        [Fact]
        public void TwoWayParsingOfKey()
        {
            var entity = new Entity();

            var guidConverter = TypeDescriptor.GetConverter(entity.Key.GetType());
            var nullableGuidConverter = TypeDescriptor.GetConverter(entity.NKey.GetType());

            var guidValue = guidConverter.ConvertTo(entity.Key, typeof(Guid));
            var nullableGuidValue = nullableGuidConverter.ConvertTo(entity.NKey, typeof(Guid));

            Assert.Equal((Guid)entity.Key, guidValue);
            Assert.Equal((Guid)entity.NKey, nullableGuidValue);

            var boxedGuidValue = (Key<Entity>)guidConverter.ConvertFrom(guidValue);
            var boxedNullableGuidValue = (Key<Entity>)guidConverter.ConvertFrom(nullableGuidValue);

            Assert.Equal(entity.Key, boxedGuidValue);
            Assert.Equal(entity.NKey, boxedNullableGuidValue);
        }

        private class Entity
        {
            public Key<Entity> Key { get; set; } = Guid.NewGuid();
            public Key<Entity>? NKey { get; set; } = Guid.NewGuid();
        }
    }
}
