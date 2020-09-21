using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.SpecificTypes
{
    public class CLRUIntTests : TestBase
    {
        [Fact]
        public async Task Insert()
        {
            var dummy = new UncommonType
            {
                CLRUInt64 = 1
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(1uL, dummy.CLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertMax()
        {
            var dummy = new UncommonType
            {
                CLRUInt64 = ulong.MaxValue
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.Equal(ulong.MaxValue, dummy.CLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullable()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").Build().QueryAsync();

            Assert.False(dummy.NCLRUInt64.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertNullableValue()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = 1
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(1uL, dummy.NCLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task InsertMaxNullableValue()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = ulong.MaxValue
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(ulong.MaxValue, dummy.NCLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task Update()
        {
            var dummy = new UncommonType();

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.CLRUInt64 = 1;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(1uL, dummy.CLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateMax()
        {
            var dummy = new UncommonType();

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.CLRUInt64 = ulong.MaxValue;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(ulong.MaxValue, dummy.CLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullable()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = 1
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRUInt64 = null;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.False(dummy.NCLRUInt64.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateMaxNullable()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = ulong.MaxValue
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRUInt64 = null;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.False(dummy.NCLRUInt64.HasValue);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateNullableValue()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRUInt64 = 1;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(1uL, dummy.NCLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }

        [Fact]
        public async Task UpdateMaxNullableValue()
        {
            var dummy = new UncommonType
            {
                NCLRUInt64 = null
            };

            Assert.Equal(1, await Database.UncommonTypes.InsertAsync(dummy));

            Database.UncommonTypes.TrackChanges(ref dummy);

            dummy.NCLRUInt64 = ulong.MaxValue;

            await Database.UncommonTypes.UpdateAsync(dummy);

            dummy = await Database.UncommonTypes.QueryInterpolatedSingle($@"SELECT * FROM ""UncommonTypes"" WHERE ""Id"" = {dummy.Id}").QueryAsync();

            Assert.Equal(ulong.MaxValue, dummy.NCLRUInt64);

            await Database.UncommonTypes.DeleteAsync(dummy);
        }
    }
}
