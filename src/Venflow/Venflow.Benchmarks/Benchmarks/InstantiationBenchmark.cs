using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Venflow.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    public class InstantiationBenchmark : BenchmarkBase
    {
        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            await InstantiateEFCoreContext();
            await InstantiateVenflowDatabase();
        }

        [Benchmark]
        public ValueTask InstantiateEFCoreContext()
        {
            return new BenchmarkDbContext().DisposeAsync();
        }

        [Benchmark]
        public ValueTask InstantiateVenflowDatabase()
        {
            return new BenchmarkDb().DisposeAsync();
        }
    }
}
