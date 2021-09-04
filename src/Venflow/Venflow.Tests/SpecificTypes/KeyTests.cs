using System;
using System.Threading.Tasks;
using Venflow.Tests.Keys;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.SpecificTypes
{
    public class KeyTests : TestBase
    {
        [Fact]
        public void Equality()
        {
            var guid = Guid.NewGuid();

            Key<KeyTests> key1 = guid;
            Key<KeyTests> key2 = guid;

            Assert.True(key1 == key2);
            Assert.False(key1 != key2);
        }

        [Fact]
        public void HashCode()
        {
            var guid = Guid.NewGuid();

            Key<KeyTests> key = guid;

            Assert.Equal(guid.GetHashCode(), key.GetHashCode());
        }

        [Fact]
        public void KeyToString()
        {
            var guid = Guid.NewGuid();

            Key<KeyTests> key = guid;

            Assert.Equal(guid.ToString(), key.ToString());
        }

        [Fact]
        public async Task Query()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                GuidKey = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""GuidKey"" = {dummy.GuidKey}").Build().QueryAsync();

            Assert.Equal(guid, (Guid)dummy.GuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task QueryNullableValue()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                NGuidKey = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""NGuidKey"" = {dummy.NGuidKey}").Build().QueryAsync();

            Assert.Equal(guid, (Guid)dummy.NGuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task Insert()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                GuidKey = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, (Guid)dummy.GuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullable()
        {
            var dummy = new UncommonType
            {
                NGuidKey = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.False(dummy.NGuidKey.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullableValue()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                NGuidKey = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.NGuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task Update()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType();

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.GuidKey = guid;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, (Guid)dummy.GuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullable()
        {
            var dummy = new UncommonType
            {
                NGuidKey = Guid.NewGuid()
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NGuidKey = null;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.False(dummy.NGuidKey.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullableValue()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                NGuidKey = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NGuidKey = guid;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.NGuidKey);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }
    }
}
