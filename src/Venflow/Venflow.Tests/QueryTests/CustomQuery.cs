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

            Assert.Equal(customResponse.Count, await Database.ExecuteAsync<long>(@"SELECT Count(""Id"") FROM ""People"""));
        }
    }

    public class CustomResponse
    {
        public long? Count { get; set; }
    }
}
