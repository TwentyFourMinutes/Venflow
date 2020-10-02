using System.Collections.Generic;
using System.Linq;
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
    public class InsertSingleWithRelationsAsyncBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EfCoreInsertSingleAsync();
            await VenflowInsertSingleAsync();
        }

        private Email GetDummyEmail()
        {
            var emails = new List<Email>();

            for (int i = 0; i < 1; i++)
            {
                var person = new Person { Name = "Test" + i.ToString(), Emails = new List<Email>() };

                for (int k = 0; k < 2; k++)
                {
                    var email = new Email { Address = person.Name + k.ToString(), Person = person, Contents = new List<EmailContent>() };

                    emails.Add(email);

                    for (int z = 0; z < 2; z++)
                    {
                        email.Contents.Add(new EmailContent { Content = email.Address + z.ToString() });
                    }
                }
            }

            return emails.First();
        }

        [Benchmark(Baseline = true)]
        public Task EfCoreInsertSingleAsync()
        {
            PersonDbContext.Emails.Add(GetDummyEmail());

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> VenflowInsertSingleAsync()
        {
            return Database.Emails.InsertAsync(GetDummyEmail());
        }

        [GlobalCleanup]
        public override async Task Cleanup()
        {
            await Database.People.TruncateAsync(Enums.ForeignTruncateOptions.Cascade);

            await base.Cleanup();
        }
    }
}