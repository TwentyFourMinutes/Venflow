using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.UpdateTests
{
    public class CustomUpdate : TestBase
    {
        [Fact]
        public async Task CopyChangeTrackingEntityAsync()
        {
            await InsertPersonWithRelationAsync();

            var person = await Database.People.QuerySingle(@"SELECT *, ""Name"" AS ""Something"" FROM ""People"" LIMIT 1")
                                              .QueryAsync();

            person!.Stuff = "Stuff";

            var oldPreson = person!;

            Database.People.TrackChanges(ref person!);

            Assert.Equal(oldPreson.Id, person.Id);
            Assert.Equal(oldPreson.Name, person.Name);
            Assert.Equal(oldPreson.Something, person.Something);
            Assert.Equal(oldPreson.SomethingElse, person.SomethingElse);
            Assert.Equal(oldPreson.Stuff, person.Stuff);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public void InstantiateChangeTrackingEntity()
        {
            var person = Database.People.GetProxiedEntity();

            Assert.NotNull(person);
            Assert.IsNotType<Person>(person);
        }

        private async Task<Person> InsertPersonWithRelationAsync()
        {
            var person = new Person { Name = "None", Emails = new List<Email> { new Email { Address = "None" } } };

            await Database.People.InsertAsync(person);

            return person;
        }
    }
}
