using BenchmarkDotNet.Attributes;
using Dapper;
using RepoDb;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class QuerySingleAsyncBenchmark : BenchmarkBase
    {
        private IQueryCommand<Person, Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = Configuration.People.QuerySingle(false).Build();

            await Configuration.GetConnection().QueryAsync<Person>(whereOrPrimaryKey: null, top: 1);

            await Configuration.People.QueryAsync(_command);

            //await PersonDbContext.People.AsNoTracking().FirstOrDefaultAsync();
        }

        //[Benchmark]
        //public Task<Person> EfCoreQuerySingleAsync()
        //{
        //    return PersonDbContext.People.AsNoTracking().FirstOrDefaultAsync();
        //}


        [Benchmark]
        public Task<Person> VenflowQuerySingleAsync()
        {
            return Configuration.People.QueryAsync(_command);
        }

        [Benchmark]
        public Task<Person> RepoDbQuerySingleAsync()
        {
            return DbConnectionExtension.QueryAsync<Person>(Configuration.GetConnection(), whereOrPrimaryKey: null, top: 1).ContinueWith(x => x.Result.First());
        }

        [Benchmark]
        public Task<Person> DapperQuerySingleAsync()
        {
            return Configuration.GetConnection().QueryFirstAsync<Person>("SELECT \"Id\", \"Name\" FROM \"Persons\" LIMIT 1");
        }


        [GlobalCleanup]
        public override async Task Cleanup()
        {
            await _command.DisposeAsync();
            await base.Cleanup();
        }
    }
}
