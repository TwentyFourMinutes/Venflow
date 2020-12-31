using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
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

        [Fact]
        public async Task QueryWithInterpolatedArray()
        {
            var person = await InsertPersonAsync();

            var queriedPerson = await Database.ExecuteInterpolatedAsync<long>($@"SELECT COUNT(1) FROM ""People"" WHERE ""People"".""Id"" IN ({new[] { person.Id, new Random().Next() }})");

            Assert.Equal(1, queriedPerson);
        }

        [Fact]
        public async Task QueryWithMissingColumnAsync()
        {
            var person = await InsertPersonAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT ""Id"" FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();

            Assert.NotNull(queriedPerson);

            Assert.Equal(person.Id, queriedPerson.Id);
            Assert.Null(queriedPerson.Name);
            Assert.NotEqual(person.Name, queriedPerson.Name);
            Assert.Null(queriedPerson.Emails);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task QueryWithMissingNullColumnAsync()
        {
            var person = await InsertPersonWithNullAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id""=@id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();

            Assert.NotNull(queriedPerson);

            Assert.Equal(person.Id, queriedPerson.Id);
            Assert.Equal(person.Name, queriedPerson.Name);
            Assert.Null(queriedPerson.Emails);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task QueryWithNoRelationAndNoResultAsync()
        {
            var queriedPerson = await Database.People.QueryInterpolatedSingle($@"SELECT * FROM ""People"" WHERE ""People"".""Id""={-1}").Build().QueryAsync();

            Assert.Null(queriedPerson);
        }

        [Fact, Priority(0)]
        public async Task QueryWithNoRelationAndIncludeAsync()
        {
            var person = await InsertPersonAsync();

            Database.People.ClearMaterializerCache();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).JoinWith(x => x.Emails).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task QueryWithRelationAsync()
        {
            var person = await InsertPersonWithRelationAsync();

            var queriedPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).JoinWith(x => x.Emails).Build().QueryAsync();

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

        [Fact]
        public async Task QueryWithInterpolationRelationAsync()
        {
            var person = await InsertPersonWithRelationAsync();

            var queriedPerson = await Database.People.QueryInterpolatedSingle($@"SELECT * FROM ""People"" >< WHERE ""People"".""Id"" = {person.Id}").AddFormatter().JoinWith(x => x.Emails).Build().QueryAsync();

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
                return Database.People.QuerySingle(@"SELECT * FROM ""People"" JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" WHERE ""People"".""Id"" = @id", new NpgsqlParameter("@id", person.Id)).Build().QueryAsync();
            });

            await Database.People.DeleteAsync(person);
        }

        private async Task<Person> InsertPersonAsync()
        {
            var person = new Person { Name = "None" };

            await Database.People.InsertAsync(person);

            return person;
        }

        private async Task<Person> InsertPersonWithNullAsync()
        {
            var person = new Person { Name = null };

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
