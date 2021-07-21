using System.Threading.Tasks;
using Venflow.Enums;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.LogTests
{
    public class LogTests : TestBase
    {
        [Fact]
        public Task LogToWrapperAsync()
        {
            var person = new Person { Name = "LogTest" };

            return Database.People.Insert().LogTo(new LoggerCallback((cmd, type, ex) =>
            {
                Assert.Null(cmd);
                Assert.Null(ex);
                Assert.Equal(CommandType.InsertSingle, type);
            })).InsertAsync(person);
        }
    }
}
