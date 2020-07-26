using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.UpdateTests
{
    public class BatchUpdate : TestBase
    {
        [Fact]
        public async Task UpdateFromQueryAsync()
        {
            var people = await InsertPeopleAsync();

            var queriedPeople = await Database.People.QueryBatch(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).TrackChanges().Build().QueryAsync();

            queriedPeople[0].Name = "NoneUpdated";
            queriedPeople[1].Name = "NoneUpdated";

            Assert.Equal("NoneUpdated", queriedPeople[0].Name);
            Assert.Equal("NoneUpdated", queriedPeople[1].Name);

            await Database.People.UpdateAsync(queriedPeople);

            queriedPeople = await Database.People.QueryBatch(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).Build().QueryAsync();

            Assert.Equal("NoneUpdated", queriedPeople[0].Name);
            Assert.Equal("NoneUpdated", queriedPeople[1].Name);

            await Database.People.DeleteAsync(queriedPeople);
        }

        [Fact]
        public async Task UpdateFromManualAsync()
        {
            var people = await InsertPeopleAsync();

            Database.People.TrackChanges(people);

            people[0].Name = "NoneUpdated";
            people[1].Name = "NoneUpdated";

            Assert.Equal("NoneUpdated", people[0].Name);
            Assert.Equal("NoneUpdated", people[1].Name);

            await Database.People.UpdateAsync(people);

            people = await Database.People.QueryBatch(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).Build().QueryAsync();

            Assert.Equal("NoneUpdated", people[0].Name);
            Assert.Equal("NoneUpdated", people[1].Name);

            await Database.People.DeleteAsync(people);
        }

        [Fact]
        public void ThrowOnNoneProxyEntityTrackChanges()
        {
            var emailContents = new List<EmailContent>
            {
                new EmailContent(),
                new EmailContent()
            };

            Assert.Throws<InvalidOperationException>(() => Database.EmailContents.TrackChanges(emailContents));
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
