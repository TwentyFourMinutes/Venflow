using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;
using Xunit.Priority;

namespace Venflow.Tests.InsertTests
{
    public class SingleInsert : TestBase
    {
        [Fact]
        public async Task InsertWithNoRelationAsync()
        {
            var person = GetPerson();

            var insertCount = await Database.People.InsertAsync(person);

            Assert.Equal(1, insertCount);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task InsertWithRelationAsync()
        {
            var person = GetPersonWithRelation();

            var insertCount = await Database.People.InsertAsync(person);

            Assert.Equal(2, insertCount);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task ReverseInsertWithRelationAsync()
        {
            var email = GetEmailWithRelation();

            var insertCount = await Database.Emails.InsertAsync(email);

            Assert.Equal(2, insertCount);

            await Database.Emails.DeleteAsync(email);
        }

        private Person GetPerson()
        {
            return new Person { Name = "None" };
        }

        private Person GetPersonWithRelation()
        {
            return new Person { Name = "None", Emails = new List<Email> { new Email { Address = "None" } } };
        }

        private Email GetEmailWithRelation()
        {
            return new Email { Address = "None", Person = new Person { Name = "None" } };
        }
    }
}
