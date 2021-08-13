using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.QueryTests
{
    public class ExpressionQuery : TestBase
    {
        [Fact]
        public async Task QueryWithExpressionAndCustomAsync()
        {
            var customResponse = await Database.Custom<CustomResponse>()
                                               .QuerySingle<Person>((_, x) => $@"SELECT Count({x.Id}) As ""Count"" FROM {x}")
                                               .QueryAsync();

            Assert.NotNull(customResponse);
            Assert.NotNull(customResponse.Count);
        }

        [Fact]
        public async Task QueryWithExpressionAsync()
        {
            var person = await InsertPersonAsync();

            var queriedPerson = await Database.People.QuerySingle(x => $"SELECT * FROM {x} WHERE {x.Id} = {person.Id}").QueryAsync();

            Assert.NotNull(queriedPerson);

            Assert.Equal(person.Id, queriedPerson.Id);
            Assert.Equal(person.Name, queriedPerson.Name);
            Assert.Null(queriedPerson.Emails);

            await Database.People.DeleteAsync(person);
        }

        [Fact]
        public async Task QueryWithRelationsExpressionAsync()
        {
            var people = await InsertPeopleWithRelationAsync();

            var queriedPeople = await Database.People.QueryBatch<Email>((x, y) => $"SELECT * FROM {x} LEFT JOIN {y} ON {y.PersonId} = {x.Id} WHERE {x.Id} = {people[0].Id} OR {x.Id} = {people[1].Id}").JoinWith(x => x.Emails).QueryAsync();

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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task QueryWithExpressionAndLocalsAsync(int parameterValue)
        {
            var value = 0;
            var descreteValue1 = 0;
            string commandText = null!;

            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {0} {value} {descreteValue1} {parameterValue}")
                .LogTo((x, _, _) =>
                {
                    commandText = x.GetUnParameterizedCommandText();
                }).QueryAsync());

            Assert.Equal($"sample query '0' '0' '0' '{parameterValue}'", commandText);

            value = 1;
            var descreteValue2 = 1;

            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {1} {value} {descreteValue2} {parameterValue}")
                 .LogTo((x, _, _) =>
                 {
                     commandText = x.GetUnParameterizedCommandText();
                 }).QueryAsync());

            Assert.Equal($"sample query '1' '1' '1' '{parameterValue}'", commandText);
        }

        [Fact]
        public async Task QueryWithExpressionAndThisAsync()
        {
            string commandText = null!;

            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {SampleMethod(0)}")
                .LogTo((x, _, _) =>
                {
                    commandText = x.GetUnParameterizedCommandText();
                }).QueryAsync());

            Assert.Equal($"sample query '0'", commandText);


            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {SampleMethod(1)}")
                 .LogTo((x, _, _) =>
                 {
                     commandText = x.GetUnParameterizedCommandText();
                 }).QueryAsync());

            Assert.Equal($"sample query '1'", commandText);
        }

        public async Task QueryWithExpressionAndLocalsAndThisAsync(int parameterValue)
        {
            var value = 0;
            var descreteValue1 = 0;
            string commandText = null!;

            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {0} {value} {descreteValue1} {SampleMethod(0)} {parameterValue}")
                .LogTo((x, _, _) =>
                {
                    commandText = x.GetUnParameterizedCommandText();
                }).QueryAsync());

            Assert.Equal($"sample query '0' '0' '0' '0' '{parameterValue}'", commandText);

            value = 1;
            var descreteValue2 = 1;

            await Assert.ThrowsAsync<PostgresException>(() => Database.People.QuerySingle(x => $"sample query {1} {value} {descreteValue2} {SampleMethod(1)} {parameterValue}")
                 .LogTo((x, _, _) =>
                 {
                     commandText = x.GetUnParameterizedCommandText();
                 }).QueryAsync());

            Assert.Equal($"sample query '1' '1' '1' '1' '{parameterValue}'", commandText);
        }

        private int SampleMethod(int value)
            => value;

        private async Task<Person> InsertPersonAsync()
        {
            var person = new Person { Name = "None" };

            await Database.People.InsertAsync(person);

            return person;
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
