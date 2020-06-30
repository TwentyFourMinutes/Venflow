using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class QueryBatchWithRelationsAsyncBenchmark : BenchmarkBase
    {
        private IQueryCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = VenflowDbConnection.Query<Person>(false).JoinWith(x => x.Emails).ThenWith(x => x.Contents).Batch(5000);

            //var people = new List<Person>();

            //for (int i = 0; i < 5000; i++)
            //{
            //    var person = new Person { Name = "QueryBatchAsync" + i.ToString(), Emails = new List<Email>()};

            //    people.Add(person);

            //    for (int k = 0; k < 2; k++)
            //    {
            //        var email = new Email { Address = person.Name + k.ToString(), PersonId = person.Id, Contents = new List<EmailContent>()};

            //        person.Emails.Add(email);

            //        for (int z = 0; z < 2; z++)
            //        {
            //            email.Contents.Add(new EmailContent { Content = email.Address + z.ToString(), EmailId = email.Id });
            //        }
            //    }
            //}

            //PersonDbContext.People.AddRange(people);

            //await PersonDbContext.SaveChangesAsync();

            await VenflowDbConnection.QueryBatchAsync(_command);

            await PersonDbContext.People.AsNoTracking().Include(x => x.Emails).ThenInclude(x => x.Contents).Take(5000).ToListAsync();

            var people = new List<Person>();
            var peopleDict = new Dictionary<int, Person>();
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(VenflowDbConnection.Connection, "SELECT * FROM (SELECT * FROM \"People\" LIMIT 10000) As People JOIN \"Emails\" As Emails On Emails.\"PersonId\" = People.\"Id\" JOIN \"EmailContents\" As EmailContents On EmailContents.\"EmailId\" = Emails.\"Id\"", (person, email, emailContent) =>
            {
                var isEmailNew = false;
                var isEmailContentNew = false;

                if (peopleDict.TryGetValue(person.Id, out var tempPerson))
                {
                    person = tempPerson;
                }
                else
                {
                    person.Emails = new List<Email>();
                    people.Add(person);
                    peopleDict.Add(person.Id, person);
                }

                if (emailDict.TryGetValue(email.Id, out var tempEmail))
                {
                    email = tempEmail;
                }
                else
                {
                    email.Contents = new List<EmailContent>();
                    isEmailNew = true;
                    emailDict.Add(email.Id, email);
                }

                if (emailContentDict.TryGetValue(emailContent.Id, out var tempEmailContent))
                {
                    emailContent = tempEmailContent;
                }
                else
                {
                    isEmailContentNew = true;
                    emailContentDict.Add(emailContent.Id, emailContent);
                }

                if (isEmailNew)
                {
                    person.Emails.Add(email);
                }

                if (isEmailContentNew)
                {
                    email.Contents.Add(emailContent);
                }

                return null;
            });
        }

        [Benchmark]
        public async Task<List<Person>> EFCoreQueryBatchAsync()
        {
            return await PersonDbContext.People.AsNoTracking().Include(x => x.Emails).ThenInclude(x => x.Contents).Take(5000).ToListAsync();
        }

        [Benchmark]
        public async Task<List<Person>> VenflowQueryBatchAsync()
        {
            return await VenflowDbConnection.QueryBatchAsync(_command);
        }

        [Benchmark]
        public async Task<List<Person>> DapperQueryBatchAsync()
        {
            var people = new List<Person>();
            var peopleDict = new Dictionary<int, Person>();
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(VenflowDbConnection.Connection, "SELECT * FROM (SELECT * FROM \"People\" LIMIT 10000) As People JOIN \"Emails\" As Emails On Emails.\"PersonId\" = People.\"Id\" JOIN \"EmailContents\" As EmailContents On EmailContents.\"EmailId\" = Emails.\"Id\"", (person, email, emailContent) =>
            {
                var isEmailNew = false;
                var isEmailContentNew = false;

                if (peopleDict.TryGetValue(person.Id, out var tempPerson))
                {
                    person = tempPerson;
                }
                else
                {
                    person.Emails = new List<Email>();
                    people.Add(person);
                    peopleDict.Add(person.Id, person);
                }

                if (emailDict.TryGetValue(email.Id, out var tempEmail))
                {
                    email = tempEmail;
                }
                else
                {
                    email.Contents = new List<EmailContent>();
                    isEmailNew = true;
                    emailDict.Add(email.Id, email);
                }

                if (emailContentDict.TryGetValue(emailContent.Id, out var tempEmailContent))
                {
                    emailContent = tempEmailContent;
                }
                else
                {
                    isEmailContentNew = true;
                    emailContentDict.Add(emailContent.Id, emailContent);
                }

                if (isEmailNew)
                {
                    person.Emails.Add(email);
                }

                if (isEmailContentNew)
                {
                    email.Contents.Add(emailContent);
                }

                return null;
            });

            return people;
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            _command.Dispose();
            return base.Cleanup();
        }
    }
}