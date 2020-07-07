using BenchmarkDotNet.Attributes;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertSingleAsyncBenchmark : BenchmarkBase
    {
        private IInsertCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = VenflowDbConnection.Insert<Person>(false).Todo();

            await VenflowDbConnection.InsertSingleAsync(_command, GetDummyPerson());

            await VenflowDbConnection.Connection.InsertAsync(GetDummyPerson());

            PersonDbContext.People.Add(GetDummyPerson());

            await PersonDbContext.SaveChangesAsync();
        }

        private Person GetDummyPerson()
        {
            return new Person { Name = "InsertSingleAsync" };
        }

        [Benchmark]
        public Task VenflowInsertSingleAsync()
        {
            return VenflowDbConnection.InsertSingleAsync(_command, GetDummyPerson());
        }

        [Benchmark]
        public Task EFCoreInsertSingleAsync()
        {
            PersonDbContext.People.Add(GetDummyPerson());

            return PersonDbContext.SaveChangesAsync();
        }


        [Benchmark]
        public Task RepoDbInsertSingleAsync()
        {
            return VenflowDbConnection.Connection.InsertAsync(GetDummyPerson());
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
