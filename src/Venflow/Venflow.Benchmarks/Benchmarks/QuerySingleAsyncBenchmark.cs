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

            await VenflowDbConnection.Connection.QueryAsync<Person>(whereOrPrimaryKey: null);
        }

        [Benchmark]
        public Task<Person> VenflowQueryOneAsync()
        {
            return VenflowDbConnection.QuerySingleAsync<Person>("SELECT \"Id\", \"Name\" FROM \"Persons\" LIMIT 1");
        }

        [Benchmark]
        public Task<Person> RepoDbQueryOneAsync()
        {
            return VenflowDbConnection.Connection.QueryAsync<Person>(whereOrPrimaryKey: null).ContinueWith(x => x.Result.First());
        }

        [Benchmark]
        public Task<Person> DapperQueryOneAsync()
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
