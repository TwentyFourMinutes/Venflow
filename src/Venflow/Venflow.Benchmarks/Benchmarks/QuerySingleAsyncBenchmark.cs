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
        private IQueryCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = VenflowDbConnection.Query<Person>().Single();

            await VenflowDbConnection.Connection.QueryAsync<Person>(whereOrPrimaryKey: null, top: 1);
            await VenflowDbConnection.QuerySingleAsync(_command);
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleAsync()
        {
            return VenflowDbConnection.QuerySingleAsync(_command);
        }

        [Benchmark]
        public Task<Person> RepoDbQuerySingleAsync()
        {
            return DbConnectionExtension.QueryAsync<Person>(VenflowDbConnection.Connection, whereOrPrimaryKey: null, top: 1).ContinueWith(x=> x.Result.First());
        }

        [Benchmark]
        public Task<Person> DapperQuerySingleAsync()
        {
            return VenflowDbConnection.Connection.QueryFirstAsync<Person>("SELECT \"Id\", \"Name\" FROM \"Persons\" LIMIT 1");
        }


        [GlobalCleanup]
        public override Task Cleanup()
        {
            _command.Dispose();
            return base.Cleanup();
        }
    }
}
