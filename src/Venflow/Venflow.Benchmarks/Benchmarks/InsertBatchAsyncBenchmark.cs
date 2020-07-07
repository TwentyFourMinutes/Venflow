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
        private IInsertCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = VenflowDbConnection.Insert<Person>(false).Compile();
            await VenflowDbConnection.InsertBatchAsync(_command, GetDummyPeople());

            await VenflowDbConnection.Connection.InsertAllAsync(GetDummyPeople());

            PersonDbContext.People.AddRange(GetDummyPeople());

            await PersonDbContext.SaveChangesAsync();
        }

        private List<Person> GetDummyPeople()
        {
            var people = new List<Person>();

            for (int i = 0; i < 100; i++)
            {
                people.Add(new Person { Name = "InsertBatchAsync" + i.ToString() });
            }

            return people;
        }

        [Benchmark]
        public Task<int> VenflowInsertBatchAsync()
        {
            return VenflowDbConnection.InsertBatchAsync(_command, GetDummyPeople());
        }

        [Benchmark]
        public Task EfCoreInsertBatchAsync()
        {
            PersonDbContext.People.AddRange(GetDummyPeople());

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task<int> RepoDbInsertBatchAsync()
        {
            return VenflowDbConnection.Connection.InsertAllAsync(GetDummyPeople());
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
