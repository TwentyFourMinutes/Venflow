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
    public class SingleQuery : TestBase
    {
        [Fact]
        public async Task QueryWithNoRelationAsync()
        {
            var person = await InsertPersonAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();

            Assert.NotNull(queriedPerson);

            Assert.Equal(person.Id, queriedPerson.Id);
            Assert.Equal(person.Name, queriedPerson.Name);
            Assert.Null(queriedPerson.Emails);

            await Database.People.DeleteAsync(person);
        }

        [Fact, Priority(0)]
        public async Task QueryWithNoRelationAndIncludeAsync()
        {
            var person = await InsertPersonAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).JoinWith(x => x.Emails).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task QueryWithRelationAsync()
        {
            var person = await InsertPersonWithRelationAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT ""People"".*, ""Emails"".""Id"" AS ""$Email$.Id"", ""Emails"".""Address"", ""Emails"".""PersonId"" FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).JoinWith(x => x.Emails).Build().QueryAsync();

            Assert.NotNull(queriedPerson);

            Assert.Equal(person.Id, queriedPerson.Id);
            Assert.Equal(person.Name, queriedPerson.Name);

            Assert.NotNull(queriedPerson.Emails);
            Assert.Single(queriedPerson.Emails);

            var email = queriedPerson.Emails[0];

            Assert.Equal(person.Emails[0].Id, email.Id);
            Assert.Equal(person.Emails[0].Address, email.Address);
            Assert.Equal(person.Emails[0].PersonId, email.PersonId);

            await Database.People.DeleteAsync(person);
        }

        [Fact, Priority(0)]
        public async Task QueryWithRelationAsyncAndNoIncludeAsync()
        {
            var person = await InsertPersonAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return Database.People.QuerySingle(@"SELECT ""People"".*, ""Emails"".""Id"" AS ""$Email$.Id"", ""Emails"".""Address"", ""Emails"".""PersonId"" FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(person);
        }

        private async Task<Person> InsertPersonAsync()
        {
            var person = new Person { Name = "None" };

            await Database.People.InsertAsync(person);

            return person;
        }

        private async Task<Person> InsertPersonWithRelationAsync()
        {
            var person = new Person { Name = "None", Emails = new List<Email> { new Email { Address = "None" } } };

            await Database.People.InsertAsync(person);

            return person;
        }
    }
}
