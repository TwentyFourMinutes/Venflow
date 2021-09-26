using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using RepoDb;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.InsertBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]

    public class InsertBatchAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int BatchCount { get; set; }

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EfCoreInsertBatchAsync();
            await VenflowInsertBatchAsync();
            await RepoDbInsertBatchAsync();
        }

        private List<Person> GetDummyPeople()
        {
            var people = new List<Person>();

            for (int i = 0; i < BatchCount; i++)
            {
                people.Add(new Person { Name = "InsertBatchAsync" + i.ToString() });
            }

            return people;
        }

        [Benchmark(Baseline = true)]
        public Task EfCoreInsertBatchAsync()
        {
            PersonDbContext.People.AddRange(GetDummyPeople());

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> VenflowInsertBatchAsync()
        {
            return Database.People.Insert().InsertAsync(GetDummyPeople());
        }

        [Benchmark]
        public Task<int> RepoDbInsertBatchAsync()
        {
            return DbConnectionExtension.InsertAllAsync(Database.GetConnection(), GetDummyPeople());
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
