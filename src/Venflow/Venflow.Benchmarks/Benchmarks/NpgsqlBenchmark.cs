using BenchmarkDotNet.Attributes;
using Npgsql;
using System.Threading.Tasks;

namespace Venflow.Benchmarks.Benchmarks
{
    public class NpgsqlBenchmark : BenchmarkBase
    {
        public NpgsqlDataReader NpgsqlDataReader { get; set; }
        public NpgsqlCommand Command { get; set; }

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

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
        public override async Task Cleanup()
        {
            await NpgsqlDataReader.DisposeAsync();

            await Command.DisposeAsync();

            await base.Cleanup();
        }
    }
}
