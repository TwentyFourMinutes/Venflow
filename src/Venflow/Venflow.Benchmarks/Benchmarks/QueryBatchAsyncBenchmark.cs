using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using RepoDb.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class QueryBatchAsyncBenchmark : BenchmarkBase
    {
        private IQueryCommand<Person> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = VenflowDbConnection.Query<Person>(false).Batch(10000);

            var persons = new List<Person>();

            for (int i = 0; i < 10000; i++)
            {
                persons.Add(new Person { Name = "QueryBatchAsync" + i.ToString() });
            }

            await VenflowDbConnection.InsertBatchAsync(persons);

            await VenflowDbConnection.QueryBatchAsync(_command);

            await VenflowDbConnection.Connection.QueryAsync<Person>(whereOrPrimaryKey: null, top: 10000);

            await PersonDbContext.People.AsNoTracking().Take(10000).ToListAsync();

            await SqlMapper.QueryAsync<Person>(VenflowDbConnection.Connection, "SELECT \"Id\", \"Name\" FROM \"People\" LIMIT 10000").ContinueWith(x => SqlMapper.AsList(x.Result));
        }

        [Benchmark]
        public Task<List<Person>> EFCoreQueryBatchAsync()
        {
            return PersonDbContext.People.AsNoTracking().Take(10000).ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchAsync()
        {
            return VenflowDbConnection.QueryBatchAsync(_command);
        }

        [Benchmark]
        public Task<List<Person>> RepoDbQueryBatchAsync()
        {
            return VenflowDbConnection.Connection.QueryAsync<Person>(whereOrPrimaryKey: null, top: 10000).ContinueWith(x => EnumerableExtension.AsList(x.Result));
        }

        [Benchmark]
        public Task<List<Person>> DapperQueryBatchAsync()
        {
            return SqlMapper.QueryAsync<Person>(VenflowDbConnection.Connection, "SELECT \"Id\", \"Name\" FROM \"People\" LIMIT 10000").ContinueWith(x => SqlMapper.AsList(x.Result));
        }


        [GlobalCleanup]
        public override Task Cleanup()
        {
            _command.Dispose();
            return base.Cleanup();
        }
    }
}