using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RepoDb;
using Venflow.Benchmarks.Benchmarks.InsertBenchmarks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.QueryBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class QuerySingleWithParameterAsyncBenchmark : BenchmarkBase
    {
        private readonly int _id = 1;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            var insertBenchmark = new InsertSingleAsyncBenchmark();

            await insertBenchmark.Setup();

            await insertBenchmark.VenflowInsertSingleAsync();
        }

        [Benchmark]
        public Task<Person?> VenflowQueryWithParameters()
        {
            return Database.People.QuerySingle(@"SELECT * FROM ""People"" WHERE ""Id"" = @p1", new NpgsqlParameter<int>("@p1", 1)).QueryAsync();
        }

        [Benchmark]
        public Task<Person?> VenflowQueryWithInterpolation()
        {
            return Database.People.QueryInterpolatedSingle($@"SELECT * FROM ""People"" WHERE ""Id"" = {1}").QueryAsync();
        }

        [Benchmark]
        public Task<Person?> VenflowQueryWithConstLambda()
        {
            return Database.People.QuerySingle(p => $"SELECT * FROM {p} WHERE {p.Id} = {1}").QueryAsync();
        }

        [Benchmark]
        public Task<Person?> VenflowQueryWithLocalLambda()
        {
            var id = 1;

            return Database.People.QuerySingle(p => $"SELECT * FROM {p} WHERE {p.Id} = {id}").QueryAsync();
        }

        [Benchmark]
        public Task<Person?> VenflowQueryWithFieldLambda()
        {
            return Database.People.QuerySingle(p => $"SELECT * FROM {p} WHERE {p.Id} = {_id}").QueryAsync();
        }

        [Benchmark]
        public Task<Person?> RepoDbQueryWithParameters()
        {
            return DbConnectionExtension.ExecuteQueryAsync<Person>(Database.GetConnection(), @"SELECT * FROM ""People"" WHERE ""Id"" = @p1", new { p1 = 1 }).ContinueWith(x => x.Result.FirstOrDefault());
        }

        [Benchmark]
        public Task<Person> DapperQueryWithParameters()
        {
            return SqlMapper.QuerySingleAsync<Person>(Database.GetConnection(), @"SELECT * FROM ""People"" LIMIT @p1", new { p1 = 1 });
        }

        [Benchmark]
        public Task<Person> DapperQueryWithBag()
        {
            var dictionary = new Dictionary<string, object>
            {
                { "@p1", 1 }
            };
            var parameters = new DynamicParameters(dictionary);

            return SqlMapper.QuerySingleAsync<Person>(Database.GetConnection(), @"SELECT * FROM ""People"" LIMIT @p1", parameters);
        }

        [Benchmark]
        public Task<Person?> EFCoreQueryWithConstLambda()
        {
            return PersonDbContext.People.FirstOrDefaultAsync(x => x.Id == 1);
        }

        [Benchmark]
        public Task<Person?> EFCoreQueryWithLocalLambda()
        {
            var id = 1;

            return PersonDbContext.People.FirstOrDefaultAsync(x => x.Id == id);
        }

        [Benchmark]
        public Task<Person?> EFCoreQueryWithFieldLambda()
        {
            return PersonDbContext.People.FirstOrDefaultAsync(x => x.Id == _id);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
