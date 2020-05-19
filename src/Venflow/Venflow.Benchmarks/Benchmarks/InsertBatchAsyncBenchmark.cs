using BenchmarkDotNet.Attributes;
using RepoDb;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertBatchAsyncBenchmark : BenchmarkBase
    {
        private List<Person> _persons;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _persons = new List<Person>();

            for (int i = 0; i < 100; i++)
            {
                _persons.Add(new Person { Name = "InsertBatchAsync" + i.ToString() });
            }

            await VenflowDbConnection.Connection.InsertAllAsync(_persons);
            await VenflowDbConnection.InsertBatchAsync(_persons);
        }

        [Benchmark]
        public ValueTask<int> VenflowInsertBatchAsync()
        {
            return VenflowDbConnection.InsertBatchAsync(_persons);
        }

        [Benchmark]
        public Task<int> RepoDbInsertBatchAsync()
        {
            return VenflowDbConnection.Connection.InsertAllAsync(_persons);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
