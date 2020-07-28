using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.DeleteBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class DeleteSingleAsyncBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await EFCoreDeleteSingleAsync();
            await VenflowDeleteSingleAsync();
            await RepoDbDeleteSingleAsync();
        }

        public async Task<Person> IterationSetupAsync()
        {
            var toDelete = new Person { Name = "toDelete" };

            await Database.People.InsertAsync(toDelete);

            return toDelete;
        }

        [Benchmark(Baseline = true)]
        public async Task EFCoreDeleteSingleAsync()
        {
            var toDelete = await IterationSetupAsync();

            PersonDbContext.People.Remove(toDelete);

            await PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public async Task VenflowDeleteSingleAsync()
        {
            var toDelete = await IterationSetupAsync();

            await Database.People.DeleteAsync(toDelete);
        }

        [Benchmark]
        public async Task RepoDbDeleteSingleAsync()
        {
            var toDelete = await IterationSetupAsync();

            await DbConnectionExtension.DeleteAsync(Database.GetConnection(), toDelete);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
