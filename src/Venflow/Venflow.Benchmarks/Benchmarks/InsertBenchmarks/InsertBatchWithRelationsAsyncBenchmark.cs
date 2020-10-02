using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.InsertBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class InsertBatchWithRelationsAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int InsertCount { get; set; }

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EfCoreInsertBatchAsync();
            await VenflowInsertBatchAsync();
        }

        private List<Email> GetDummyEmails()
        {
            var emails = new List<Email>();

            for (int i = 0; i < InsertCount; i++)
            {
                var person = new Person { Name = "Test" + i.ToString(), Emails = new List<Email>() };

                for (int k = 0; k < 2; k++)
                {
                    var email = new Email { Address = person.Name + k.ToString(), Person = person, Contents = new List<EmailContent>() };

                    person.Emails.Add(email);

                    emails.Add(email);

                    for (int z = 0; z < 2; z++)
                    {
                        email.Contents.Add(new EmailContent { Content = email.Address + z.ToString(), Email = email });
                    }
                }
            }

            return emails;
        }

        [Benchmark(Baseline = true)]
        public Task EfCoreInsertBatchAsync()
        {
            PersonDbContext.Emails.AddRange(GetDummyEmails());

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> VenflowInsertBatchAsync()
        {
            return Database.Emails.InsertAsync(GetDummyEmails());
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}