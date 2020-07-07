using BenchmarkDotNet.Attributes;
using RepoDb;
using System.Collections.Generic;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertBatchAsyncBenchmark : BenchmarkBase
    {
        private List<Person> _people;
        private IInsertCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _people = new List<Person>();

            for (int i = 0; i < 100; i++)
            {
                _people.Add(new Person { Name = "InsertBatchAsync" + i.ToString() });
            }

            _command = VenflowDbConnection.Insert<Person>(false).Todo();
            await VenflowDbConnection.InsertBatchAsync(_command, _people);

            await VenflowDbConnection.Connection.InsertAllAsync(_people);

            PersonDbContext.People.AddRange(_people);

            await PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> VenflowInsertBatchAsync()
        {
            return VenflowDbConnection.InsertBatchAsync(_command, _people);
        }

        [Benchmark]
        public Task EfCoreInsertBatchAsync()
        {
            PersonDbContext.People.AddRange(_people);

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> RepoDbInsertBatchAsync()
        {
            return VenflowDbConnection.Connection.InsertAllAsync(_people);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
