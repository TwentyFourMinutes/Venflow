using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.InsertTests
{
    public class BatchInsert : TestBase
    {
        [Fact]
        public async Task InsertWithNoRelationAsync()
        {
            var people = GetPeople();

            var insertCount = await Database.People.InsertAsync(people);

            Assert.Equal(2, insertCount);

            Assert.Equal(2, await Database.People.DeleteAsync(people));
        }

        [Fact]
        public async Task InsertWithRelationAsync()
        {
            var people = GetPeopleWithRelation();

            var insertCount = await Database.People.InsertAsync(people);

            Assert.Equal(4, insertCount);

            Assert.Equal(2, await Database.People.DeleteAsync(people));
        }

        [Fact]
        public async Task ReverseInsertWithRelationAsync()
        {
            var emails = GetEmailsWithRelation();

            var insertCount = await Database.Emails.InsertAsync(emails);

            Assert.Equal(4, insertCount);

            Assert.Equal(2, await Database.Emails.DeleteAsync(emails));
        }

        private List<Person> GetPeople()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1" });
            people.Add(new Person { Name = "None2" });

            return people;
        }

        private List<Person> GetPeopleWithRelation()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1", Emails = new List<Email> { new Email { Address = "None1" } } });
            people.Add(new Person { Name = "None2", Emails = new List<Email> { new Email { Address = "None2" } } });

            return people;
        }

        private List<Email> GetEmailsWithRelation()
        {
            var emails = new List<Email>();

            emails.Add(new Email { Address = "None1", Person = new Person { Name = "None1" } });
            emails.Add(new Email { Address = "None2", Person = new Person { Name = "None2" } });

            return emails;
        }
    }
}