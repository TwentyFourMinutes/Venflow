using BenchmarkDotNet.Attributes;
using Dapper;
using RepoDb;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    public class QuerySingleAsyncBenchmark
    {
        public MyDbConfiguration Configuration { get; set; }
        public VenflowDbConnection VenflowDbConnection { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            Configuration = new MyDbConfiguration();

            PostgreSqlBootstrap.Initialize();
            ClassMapper.Add<Person>("\"Persons\"");

            VenflowDbConnection = await Configuration.NewConnectionScopeAsync();

            await DbConnectionExtension.QueryAsync<Person>(VenflowDbConnection.Connection, whereOrPrimaryKey: null, top: 1);
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleAsync()
        {
            return VenflowDbConnection.QuerySingleAsync<Person>("SELECT \"Id\", \"Name\" FROM \"Persons\" LIMIT 1");
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
        public async Task Cleanup()
        {
            Configuration = new MyDbConfiguration();

            await VenflowDbConnection.DisposeAsync();
        }
    }
}
