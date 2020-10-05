using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

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

            Assert.Equal(1, await Database.People.DeleteAsync(person));
        }

        [Fact]
        public async Task InsertWithNoRelationNoPKAsync()
        {
            var user = new User { Id = 0, Name = "Foo" };

            var insertCount = await Database.Users.InsertAsync(user);

            Assert.Equal(1, insertCount);

            Assert.Equal(1, await Database.Users.DeleteAsync(user));
        }

        [Fact]
        public async Task InsertWithPartialRelationAsync()
        {
            var person = GetPersonWithFullRelation();

            var insertCount = await Database.People.Insert().InsertWith(x => x.Emails).InsertAsync(person);

            Assert.Equal(2, insertCount);

            Assert.Equal(1, await Database.People.DeleteAsync(person));
        }

        [Fact]
        public async Task InsertWithPartialRelationReversedAsync()
        {
            var email = GetEmailWithFullRelation();

            var insertCount = await Database.Emails.Insert().InsertWith(x => x.Person).InsertAsync(email);

            Assert.Equal(2, insertCount);

            Assert.Equal(1, await Database.People.DeleteAsync(email.Person));
        }

        [Fact]
        public async Task InsertWithRelationAsync()
        {
            var person = GetPersonWithRelation();

            var insertCount = await Database.People.InsertAsync(person);

            Assert.Equal(2, insertCount);

            Assert.Equal(1, await Database.People.DeleteAsync(person));
        }

        [Fact]
        public async Task ReverseInsertWithRelationAsync()
        {
            var email = GetEmailWithRelation();

            var insertCount = await Database.Emails.InsertAsync(email);

            Assert.Equal(2, insertCount);

            Assert.Equal(1, await Database.Emails.DeleteAsync(email));
        }

        private Person GetPerson()
        {
            return new Person { Name = "None" };
        }

        private Person GetPersonWithRelation()
        {
            return new Person { Name = "None", Emails = new List<Email> { new Email { Address = "None" } } };
        }

        private Person GetPersonWithFullRelation()
        {
            return new Person { Name = "None", Emails = new List<Email> { new Email { Address = "None", Contents = new List<EmailContent> { new EmailContent { Content = "None" } } } } };
        }

        private Email GetEmailWithRelation()
        {
            return new Email { Address = "None", Person = new Person { Name = "None" } };
        }

        private Email GetEmailWithFullRelation()
        {
            return new Email { Address = "None", Person = new Person { Name = "None" }, Contents = new List<EmailContent> { new EmailContent { Content = "None" } } };
        }
    }
}
