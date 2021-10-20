using System.Collections.Generic;
using System.Linq;
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

            var insertCount = await Database.People.Insert().InsertAsync(people);

            Assert.Equal(2, insertCount);

            Assert.Equal(2, await Database.People.DeleteAsync(people));
        }

        [Fact]
        public async Task InsertWithNoRelationNoPKAsync()
        {
            var users = new List<User>
            {
                new User{ Id = 0, Name = "Foo" },
                new User { Id = 1, Name = "Bar" }
            };

            var insertCount = await Database.Users.Insert().InsertAsync(users);

            Assert.Equal(2, insertCount);

            Assert.Equal(2, await Database.Users.DeleteAsync(users));
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
        public async Task InsertWithRelationNoPKAsync()
        {
            var users = new List<User>
            {
                new User { Id = 20, Name = "Foo", Blogs = new List<Blog> { new Blog { Id = 21, Topic = "BazFoo" } } },
                new User { Id = 22, Name = "Bar",  Blogs = new List<Blog> { new Blog { Id = 23, Topic = "BazBar" } }}
            };

            var insertCount = await Database.Users.InsertAsync(users);

            Assert.Equal(4, insertCount);

            Assert.Equal(2, await Database.Users.DeleteAsync(users));
        }

        [Fact]
        public async Task InsertWithPartialRelationAsync()
        {
            var people = GetPeopleWithFullRelation();

            var insertCount = await Database.People.Insert().With(x => x.Emails).InsertAsync(people);

            Assert.Equal(4, insertCount);

            Assert.Equal(2, await Database.People.DeleteAsync(people));
        }

        [Fact]
        public async Task InsertWithPartialRelationReversedAsync()
        {
            var emails = GetEmailsWithFullRelation();

            var insertCount = await Database.Emails.Insert().With(x => x.Person).InsertAsync(emails);

            Assert.Equal(3, insertCount);

            Assert.Equal(1, await Database.People.DeleteAsync(emails.First().Person));
        }

        [Fact]
        public async Task ReverseInsertWithRelationAsync()
        {
            var emails = GetEmailsWithRelation();

            var insertCount = await Database.Emails.InsertAsync(emails);

            Assert.Equal(3, insertCount);

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

        private List<Person> GetPeopleWithFullRelation()
        {
            var people = new List<Person>();

            people.Add(new Person { Name = "None1", Emails = new List<Email> { new Email { Address = "None1", Contents = new List<EmailContent> { new EmailContent { Content = "None1" } } } } });
            people.Add(new Person { Name = "None2", Emails = new List<Email> { new Email { Address = "None2", Contents = new List<EmailContent> { new EmailContent { Content = "None2" } } } } });

            return people;
        }

        private List<Email> GetEmailsWithFullRelation()
        {
            var emails = new List<Email>();

            var person = new Person { Name = "None" };

            emails.Add(new Email { Address = "None1", Person = person, Contents = new List<EmailContent> { new EmailContent { Content = "None1" } } });
            emails.Add(new Email { Address = "None2", Person = person, Contents = new List<EmailContent> { new EmailContent { Content = "None2" } } });

            return emails;
        }

        private List<Email> GetEmailsWithRelation()
        {
            var emails = new List<Email>();

            var person = new Person { Name = "None" };

            emails.Add(new Email { Address = "None1", Person = person });
            emails.Add(new Email { Address = "None2", Person = person });

            return emails;
        }
    }
}
