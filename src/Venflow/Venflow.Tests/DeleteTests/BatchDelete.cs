using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.DeleteTests
{
    public class BatchDelete : TestBase
    {
        [Fact]
        public async Task DeleteAsync()
        {
            var people = await InsertPeopleAsync();

            await Database.People.DeleteAsync(people);

            Assert.Empty(await Database.People.QueryBatch(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).Build().QueryAsync());
        }

        private async Task<List<Person>> InsertPeopleAsync()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1" });
            people.Add(new Person { Name = "None2" });

            await Database.People.InsertAsync(people);

            return people;
        }
    }
}
