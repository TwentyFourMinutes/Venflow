using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;
using Xunit.Priority;

namespace Venflow.Tests.QueryTests
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class BatchQuery : TestBase
    {
        [Fact, Priority(0)]
        public async Task QueryWithRelationAsyncAndNoIncludeAsync()
        {
            var people = await InsertPeopleAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return Database.People.QueryBatch(@"SELECT ""People"".*, ""Emails"".""Id"" AS ""$Email$.Id"", ""Emails"".""Address"", ""Emails"".""PersonId"" FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(people);
        }

        [Fact, Priority(1)]
        public async Task QueryWithNoRelationAndIncludeAsync()
        {
            var people = await InsertPeopleAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).JoinWith(x => x.Emails).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(people);
        }


        [Fact]
        public async Task QueryWithNoRelationAsync()
        {
            var people = await InsertPeopleAsync();

            var queriedPeople = await Database.People.QueryBatch(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).Build().QueryAsync();

            Assert.NotNull(queriedPeople);
            Assert.Equal(people.Count, queriedPeople.Count);

            for (int i = 0; i < queriedPeople.Count; i++)
            {
                Assert.Equal(people[i].Id, queriedPeople[i].Id);
                Assert.Equal(people[i].Name, queriedPeople[i].Name);
                Assert.Null(queriedPeople[i].Emails);
            }

            await Database.People.DeleteAsync(people);
        }

        [Fact]
        public async Task QueryWithRelationAsync()
        {
            var people = await InsertPeopleWithRelationAsync();

            var queriedPeople = await Database.People.QueryBatch(@"SELECT ""People"".*, ""Emails"".""Id"" AS ""$Email$.Id"", ""Emails"".""Address"", ""Emails"".""PersonId"" FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id""=@id1 OR ""People"".""Id""=@id2", new NpgsqlParameter("@id1", people[0].Id), new NpgsqlParameter("@id2", people[1].Id)).JoinWith(x => x.Emails).Build().QueryAsync();

            Assert.NotNull(queriedPeople);
            Assert.Equal(people.Count, queriedPeople.Count);

            for (int i = 0; i < queriedPeople.Count; i++)
            {
                Assert.Equal(people[i].Id, queriedPeople[i].Id);
                Assert.Equal(people[i].Name, queriedPeople[i].Name);

                Assert.NotNull(queriedPeople[i].Emails);
                Assert.Single(queriedPeople[i].Emails);

                var email = queriedPeople[i].Emails[0];

                Assert.Equal(people[i].Emails[0].Id, email.Id);
                Assert.Equal(people[i].Emails[0].Address, email.Address);
                Assert.Equal(people[i].Emails[0].PersonId, email.PersonId);
            }

            await Database.People.DeleteAsync(people);
        }

        private async Task<List<Person>> InsertPeopleAsync()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1" });
            people.Add(new Person { Name = "None2" });

            await Database.People.InsertAsync(people);

            return people;
        }

        private async Task<List<Person>> InsertPeopleWithRelationAsync()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1", Emails = new List<Email> { new Email { Address = "None1" } } });
            people.Add(new Person { Name = "None2", Emails = new List<Email> { new Email { Address = "None2" } } });

            await Database.People.InsertAsync(people);

            return people;
        }
    }
}
