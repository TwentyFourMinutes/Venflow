using BenchmarkDotNet.Attributes;
using Npgsql;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    public class NpgsqlBenchmark
    {
        public MyDbConfiguration Configuration { get; set; }
        public VenflowDbConnection VenflowDbConnection { get; set; }
        public NpgsqlDataReader NpgsqlDataReader { get; set; }
        public NpgsqlCommand Command { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            Configuration = new MyDbConfiguration();

            PostgreSqlBootstrap.Initialize();
            ClassMapper.Add<Person>("\"Persons\"");

            VenflowDbConnection = await Configuration.NewConnectionScopeAsync();

            Command = new NpgsqlCommand("SELECT \"Id\", \"Name\" FROM \"Persons\" LIMIT 1", VenflowDbConnection.Connection);

            NpgsqlDataReader = await Command.ExecuteReaderAsync();
            await NpgsqlDataReader.ReadAsync();

        }

        [Benchmark]
        public int GetInt32()
        {
            return NpgsqlDataReader.GetInt32(0);
        }

        [Benchmark]
        public int GetObjectInt32()
        {
            return (int)NpgsqlDataReader.GetValue(0);
        }

        [Benchmark]
        public int GetTInt32()
        {
            return NpgsqlDataReader.GetFieldValue<int>(0);
        }

        [Benchmark]
        public string GetString()
        {
            return NpgsqlDataReader.GetString(1);
        }

        [Benchmark]
        public string GetObjectString()
        {
            return (string)NpgsqlDataReader.GetValue(1);
        }

        [Benchmark]
        public string GetTString()
        {
            return NpgsqlDataReader.GetFieldValue<string>(1);
        }

        [GlobalCleanup]
        public async Task Cleanup()
        {
            Configuration = new MyDbConfiguration();

            await NpgsqlDataReader.DisposeAsync();

            await Command.DisposeAsync();

            await VenflowDbConnection.DisposeAsync();
        }
    }
}
