using System;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.UpdateTests
{
    public class SingleUpdate : TestBase
    {
        [Fact]
        public async Task UpdateFromQueryAsync()
        {
            var person = await InsertPersonAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).TrackChanges().Build().QueryAsync();

            queriedPerson.Name = "NoneUpdated";

            Assert.Equal("NoneUpdated", queriedPerson.Name);

            await Database.People.UpdateAsync(queriedPerson);

            queriedPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();

            Assert.Equal("NoneUpdated", queriedPerson.Name);

            await Database.People.DeleteAsync(queriedPerson);
        }

        [Fact]
        public async Task UpdateFromManualAsync()
        {
            var person = await InsertPersonAsync();

            Database.People.TrackChanges(ref person);

            person.Name = "NoneUpdated";

            Assert.Equal("NoneUpdated", person.Name);

            await Database.People.UpdateAsync(person);

            person = await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();

            Assert.Equal("NoneUpdated", person.Name);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public void ThrowOnNoneProxyEntityTrackChanges()
        {
            var emailContent = new EmailContent();

            Assert.Throws<InvalidOperationException>(() => Database.EmailContents.TrackChanges(ref emailContent));
        }

        private async Task<Person> InsertPersonAsync()
        {
            var person = new Person { Name = "None" };

            await Database.People.InsertAsync(person);

            return person;
        }
    }
}
