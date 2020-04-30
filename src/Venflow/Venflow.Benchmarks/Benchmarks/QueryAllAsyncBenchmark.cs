using BenchmarkDotNet.Attributes;
using Dapper;
using RepoDb;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
	public class QueryAllAsyncBenchmark
	{
		public MyDbConfiguration Configuration { get; set; }
		public VenflowDbConnection VenflowDbConnection { get; set; }
		public VenflowCommand<Person> Command { get; set; }

		[GlobalSetup]
		public async Task Setup()
		{
			Configuration = new MyDbConfiguration();

			PostgreSqlBootstrap.Initialize();
			ClassMapper.Add<Person>("\"Persons\"");

			VenflowDbConnection = await Configuration.NewConnectionScopeAsync();

			await VenflowDbConnection.Connection.QueryAllAsync<Person>();
			Command = VenflowDbConnection.CompileQueryAllCommand<Person>("SELECT \"Id\", \"Name\" FROM \"Persons\"");
			await VenflowDbConnection.QueryAllAsync(Command);
		}

		[Benchmark]
		public Task<ICollection<Person>> VenflowQueryAllAsync()
		{
			return VenflowDbConnection.QueryAllAsync(Command);
		}

		[Benchmark]
		public Task<List<Person>> RepoDbQueryAllAsync()
		{
			return VenflowDbConnection.Connection.QueryAllAsync<Person>().ContinueWith(x => x.Result.ToList());
		}

		[Benchmark]
		public Task<List<Person>> DapperQueryAllAsync()
		{
			return SqlMapper.QueryAsync<Person>(VenflowDbConnection.Connection, "SELECT \"Id\", \"Name\" FROM \"Persons\"").ContinueWith(x => x.Result.ToList());
		}


		[GlobalCleanup]
		public async Task Cleanup()
		{
			Configuration = new MyDbConfiguration();

			await VenflowDbConnection.DisposeAsync();

			Command.Dispose();
		}
	}
}
