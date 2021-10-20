using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Venflow.Tests.QueryTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class CustomQuery : TestBase
    {
        [Fact]
        public async Task QueryWithNoRelationAsync()
        {
            var customResponse = await Database.Custom<CustomResponse>()
                                               .QuerySingle(@"SELECT Count(""Id"") As ""Count"" FROM ""People""")
                                               .QueryAsync();

            Assert.NotNull(customResponse);
            Assert.NotNull(customResponse.Count);
        }

        [Fact]
        public async Task QueryBatchWithNoRelationAsync()
        {
            var customResponse = await Database.Custom<CustomResponse>()
                                               .QueryBatch(@"SELECT Count(""Id"") As ""Count"" FROM ""People""")
                                               .QueryAsync();

            Assert.Single(customResponse);
            Assert.NotNull(customResponse[0]);
            Assert.NotNull(customResponse[0].Count);
        }
    }

    public class CustomResponse
    {
        public long? Count { get; set; }
    }
}
