using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests
{
    public class TableTests : TestBase
    {
        [Fact]
        public async Task Count()
        {
            await using var transaction = await Database.BeginTransactionAsync();

            var intialCount = await Database.People.CountAsync();

            await Database.People.InsertAsync(new[] { new Person(), new Person(), new Person() });

            Assert.True(await Database.People.CountAsync() >= intialCount + 3);

            transaction.Rollback();
        }

        [Fact]
        public async Task Truncate()
        {
            await using var transaction = await Database.BeginTransactionAsync();

            await Database.People.InsertAsync(new[] { new Person(), new Person(), new Person() });

            await Database.People.TruncateAsync(Enums.ForeignTruncateOptions.Cascade);

            Assert.Equal(0, await Database.People.CountAsync());

            transaction.Rollback();
        }
    }
}
