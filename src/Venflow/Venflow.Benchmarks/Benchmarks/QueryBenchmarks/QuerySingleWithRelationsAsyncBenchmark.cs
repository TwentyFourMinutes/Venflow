using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using Venflow.Benchmarks.Benchmarks.InsertBenchmarks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.QueryBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class QuerySingleWithRelationsAsyncBenchmark : BenchmarkBase
    {
        private string sql => @"SELECT * FROM (SELECT * FROM ""People"" LIMIT 1) AS ""People"" INNER JOIN ""Emails"" ON ""Emails"".""PersonId"" = ""People"".""Id"" INNER JOIN ""EmailContents"" ON ""EmailContents"".""EmailId"" = ""Emails"".""Id""";

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            var insertBenchmark = new InsertBatchWithRelationsAsyncBenchmark();

            await insertBenchmark.Setup();

            insertBenchmark.InsertCount = 10000;

            await insertBenchmark.VenflowInsertBatchAsync();

            await insertBenchmark.Database.DisposeAsync();

            await insertBenchmark.PersonDbContext.DisposeAsync();

            await EfCoreQuerySingleAsync();
            await EfCoreQuerySingleNoChangeTrackingAsync();
            await VenflowQuerySingleAsync();
            await VenflowQuerySingleNoChangeTrackingAsync();
            await RecommendedDapperQuerySingleAsync();
            await CustomDapperQuerySingleAsync();
        }

        [Benchmark(Baseline = true)]
        public Task<Person> EfCoreQuerySingleAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            return PersonDbContext.People.Include(x => x.Emails).ThenInclude(x => x.Contents).FirstOrDefaultAsync();
        }

        [Benchmark]
        public Task<Person> EfCoreQuerySingleNoChangeTrackingAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return PersonDbContext.People.AsNoTracking().Include(x => x.Emails).ThenInclude(x => x.Contents).FirstOrDefaultAsync();
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleAsync()
        {
            return Database.People.QuerySingle(sql).JoinWith(x => x.Emails).ThenWith(x => x.Contents).TrackChanges().Build().QueryAsync();
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleNoChangeTrackingAsync()
        {
            return Database.People.QuerySingle(sql).JoinWith(x => x.Emails).ThenWith(x => x.Contents).Build().QueryAsync();
        }

        [Benchmark]
        public async Task<Person> RecommendedDapperQuerySingleAsync()
        {
            var peopleDict = new Dictionary<int, Person>();
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            var person = (await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(Database.GetConnection(), sql, (person, email, emailContent) =>
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
            })).Distinct().FirstOrDefault();

            return person;
        }

        [Benchmark]
        public async Task<Person> CustomDapperQuerySingleAsync()
        {
            Person? resultPerson = default;
            var emailDict = new Dictionary<int, Email>();
            var emailContentDict = new Dictionary<int, EmailContent>();

            await Dapper.SqlMapper.QueryAsync<Person, Email, EmailContent, Person>(Database.GetConnection(), sql, (person, email, emailContent) =>
            {
                var isEmailNew = false;
                var isEmailContentNew = false;

                if (resultPerson is null)
                {
                    resultPerson = person;

                    person.Emails = new List<Email>();
                }
                else
                {
                    person = resultPerson;
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

            return resultPerson;
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}