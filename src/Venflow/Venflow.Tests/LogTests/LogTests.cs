using System.Threading.Tasks;
using Venflow.Enums;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.LogTests
{
    public class LogTests : TestBase
    {
        [Fact]
        public async Task LogToWrapperAsync()
        {
            var person = new Person { Name = "LogTest" };

            var logCount = 0;

            await Database.People.Insert().LogTo(new LoggerCallback((cmd, type, ex) =>
            {
                Assert.NotNull(cmd);
                Assert.Null(ex);
                Assert.Equal(CommandType.InsertSingle, type);

                logCount++;
            })).InsertAsync(person);

            Assert.Equal(1, logCount);
        }
    }
}
