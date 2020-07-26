using Npgsql;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.DeleteTests
{
    public class SingleDelete : TestBase
    {
        [Fact]
        public async Task DeleteAsync()
        {
            var person = await InsertPersonAsync();

            await Database.People.DeleteAsync(person);

            Assert.Null(await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync());
        }

        private async Task<Person> InsertPersonAsync()
        {
            var person = new Person { Name = "None" };

            await Database.People.InsertAsync(person);

            return person;
        }
    }
}
