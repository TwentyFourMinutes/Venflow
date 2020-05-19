using BenchmarkDotNet.Attributes;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertSingleAsyncBenchmark : BenchmarkBase
    {
        public Person _person;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _person = new Person { Name = "InsertSingleAsync" };

            await VenflowDbConnection.Connection.InsertAsync(_person);
            await VenflowDbConnection.InsertSingleAsync(_person);
        }

        [Benchmark]
        public Task VenflowInsertSingleAsync()
        {
            return VenflowDbConnection.InsertSingleAsync(_person);
        }

        [Benchmark]
        public Task RepoDbInsertSingleAsync()
        {
            return VenflowDbConnection.Connection.InsertAsync(_person);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
