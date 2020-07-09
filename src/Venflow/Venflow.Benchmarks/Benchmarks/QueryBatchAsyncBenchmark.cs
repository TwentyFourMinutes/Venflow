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
        private IQueryCommand<Person, List<Person>> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            _command = Configuration.People.QueryBatch(20000, false).Build();

            await Configuration.People.QueryAsync(_command);

            await Configuration.GetConnection().QueryAsync<Person>(whereOrPrimaryKey: null, top: 20000);

            //await PersonDbContext.People.AsNoTracking().Take(5000).ToListAsync();

            await SqlMapper.QueryAsync<Person>(Configuration.GetConnection(), "SELECT \"Id\", \"Name\" FROM \"People\" LIMIT 20000").ContinueWith(x => SqlMapper.AsList(x.Result));
        }

        //[Benchmark]
        //public Task<List<Person>> EFCoreQueryBatchAsync()
        //{
        //    return PersonDbContext.People.AsNoTracking().Take(5000).ToListAsync();
        //}

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchAsync()
        {
            return Configuration.People.QueryAsync(_command);
        }

        [Benchmark]
        public Task<List<Person>> RepoDbQueryBatchAsync()
        {
            return Configuration.GetConnection().QueryAsync<Person>(whereOrPrimaryKey: null, top: 20000).ContinueWith(x => EnumerableExtension.AsList(x.Result));
        }

        [Benchmark]
        public Task<List<Person>> DapperQueryBatchAsync()
        {
            return SqlMapper.QueryAsync<Person>(Configuration.GetConnection(), "SELECT \"Id\", \"Name\" FROM \"People\" LIMIT 20000").ContinueWith(x => SqlMapper.AsList(x.Result));
        }


        [GlobalCleanup]
        public override async Task Cleanup()
        {
            await _command.DisposeAsync();
            await base.Cleanup();
        }
    }
}