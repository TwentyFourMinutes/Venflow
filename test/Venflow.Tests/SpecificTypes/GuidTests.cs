using System;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.SpecificTypes
{
    public class GuidTests : TestBase
    {
        [Fact]
        public async Task Insert()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                CLRGuid = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.CLRGuid);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullable()
        {
            var dummy = new UncommonType
            {
                NCLRGuid = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.False(dummy.NCLRGuid.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullableValue()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                NCLRGuid = guid
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.NCLRGuid);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task Update()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType();

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.CLRGuid = guid;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.CLRGuid);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullable()
        {
            var dummy = new UncommonType
            {
                NCLRGuid = Guid.NewGuid()
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRGuid = null;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.False(dummy.NCLRGuid.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullableValue()
        {
            var guid = Guid.NewGuid();

            var dummy = new UncommonType
            {
                NCLRGuid = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRGuid = guid;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(guid, dummy.NCLRGuid);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }
    }
}
