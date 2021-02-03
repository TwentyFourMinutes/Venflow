using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Venflow.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
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
