using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.QueryBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class QueryBatchWithRelationsAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int QueryCount { get; set; }

        private string sql => @"SELECT * FROM (SELECT * FROM ""People"" LIMIT " + QueryCount + @") AS ""People"" INNER JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" INNER JOIN ""EmailContents"" ON ""EmailContents"".""EmailId"" = ""Emails"".""Id""";

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EfCoreQueryBatchAsync();
            await EfCoreQueryBatchNoChangeTrackingAsync();
            await VenflowQueryBatchAsync();
            await VenflowQueryBatchNoChangeTrackingAsync();
            await RecommendedDapperQueryBatchAsync();
            await CustomDapperQueryBatchAsync();
        }

        [Benchmark]
        public Task<List<Person>> EfCoreQueryBatchAsync()
        {
            return PersonDbContext.People.Include(x => x.Emails).ThenInclude(x => x.Contents).Take(QueryCount).ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> EfCoreQueryBatchNoChangeTrackingAsync()
        {
            return PersonDbContext.People.AsNoTracking().Include(x => x.Emails).ThenInclude(x => x.Contents).Take(QueryCount).ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchAsync()
        {
            return Database.People.QueryBatch(sql).JoinWith(x=> x.Emails).ThenWith(x=> x.Contents).TrackChanges().Build().QueryAsync();
        }

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchNoChangeTrackingAsync()
        {
            return Database.People.QueryBatch(sql).JoinWith(x => x.Emails).ThenWith(x => x.Contents).Build().QueryAsync();
        }

        [Benchmark]
        public async Task<List<Person>> RecommendedDapperQueryBatchAsync()
        {
            var peopleDict = new Dictionary<int, Person>();
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            var people = (await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(Database.GetConnection(), "SELECT * FROM (SELECT * FROM \"People\" LIMIT 5000) As People JOIN \"Emails\" As Emails On Emails.\"PersonId\" = People.\"Id\" JOIN \"EmailContents\" As EmailContents On EmailContents.\"EmailId\" = Emails.\"Id\"", (person, email, emailContent) =>
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

                return person;
            })).Distinct().ToList();

            return people;
        }

        [Benchmark]
        public async Task<List<Person>> CustomDapperQueryBatchAsync()
        {
            var people = new List<Person>();
            var peopleDict = new Dictionary<int, Person>();
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(Database.GetConnection(), "SELECT * FROM (SELECT * FROM \"People\" LIMIT 5000) As People JOIN \"Emails\" As Emails On Emails.\"PersonId\" = People.\"Id\" JOIN \"EmailContents\" As EmailContents On EmailContents.\"EmailId\" = Emails.\"Id\"", (person, email, emailContent) =>
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
            return base.Cleanup();
        }
    }
}