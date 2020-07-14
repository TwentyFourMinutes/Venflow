using BenchmarkDotNet.Attributes;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InstantiationBenchmark
    {
        [GlobalSetup]
        public async ValueTask Setup()
        {
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
