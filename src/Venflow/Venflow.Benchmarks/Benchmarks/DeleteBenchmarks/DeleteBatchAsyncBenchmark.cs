using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using RepoDb;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.DeleteBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class DeleteBatchAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int DeleteCount { get; set; }

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EFCoreDeleteBatchAsync();
            await VenflowDeleteBatchAsync();
            await RepoDbDeleteBatchAsync();
        }

        public async Task<List<Person>> IterationSetupAsync()
        {
            var toDelete = new List<Person>();

            for (int i = 0; i < DeleteCount; i++)
            {
                toDelete.Add(new Person { Name = "toDelete" + i.ToString() });
            }

            await Database.People.Insert().InsertAsync(toDelete);

            return toDelete;
        }

        [Benchmark(Baseline = true)]
        public async Task EFCoreDeleteBatchAsync()
        {
            var toDelete = await IterationSetupAsync();

            PersonDbContext.People.RemoveRange(toDelete);

            await PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public async Task VenflowDeleteBatchAsync()
        {
            var toDelete = await IterationSetupAsync();

            await Database.People.DeleteAsync(toDelete);
        }

        [Benchmark]
        public async Task RepoDbDeleteBatchAsync()
        {
            var toDelete = await IterationSetupAsync();

            await DbConnectionExtension.DeleteAllAsync(Database.GetConnection(), toDelete);
        }


        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
