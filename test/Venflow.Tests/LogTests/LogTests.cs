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

        [Fact]
        public async Task LogMultipleToWrapperAsync()
        {
            var person = new Person { Name = "LogTest", Emails = new Email[] { new Email() { Address = "LogTest" } } };

            var logCount = 0;

            await Database.People.Insert().With(x => x.Emails).LogTo(new LoggerCallback((cmd, type, ex) =>
             {
                 Assert.NotNull(cmd);
                 Assert.Null(ex);
                 Assert.Equal(CommandType.InsertSingle, type);

                 logCount++;
             })).InsertAsync(person);

            Assert.Equal(2, logCount);
        }

        [Fact]
        public async Task DoNotLogMultipleToWrapperAsync()
        {
            var person = new Person { Name = "LogTest" };

            var logCount = 0;

            await Database.People.Insert().WithAll().LogTo(new LoggerCallback((cmd, type, ex) =>
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
