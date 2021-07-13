using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using RepoDb.Extensions;
using Venflow.Benchmarks.Benchmarks.InsertBenchmarks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.QueryBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]

    public class QueryBatchAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int BatchCount { get; set; }

        private string sql => @"SELECT ""Id"", ""Name"" FROM ""People"" LIMIT " + BatchCount;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            var insertBenchmark = new InsertBatchWithRelationsAsyncBenchmark();

            await insertBenchmark.Setup();

            insertBenchmark.BatchCount = 10000;

            await insertBenchmark.VenflowInsertBatchAsync();

            await insertBenchmark.Database.DisposeAsync();

            await insertBenchmark.PersonDbContext.DisposeAsync();

            await EfCoreQueryBatchAsync();
            await EfCoreQueryBatchNoChangeTrackingAsync();
            await EfCoreQueryBatchRawNoChangeTrackingAsync();
            await VenflowQueryBatchAsync();
            await VenflowQueryBatchNoChangeTrackingAsync();
            await RepoDbQueryBatchAsync();
            await DapperQueryBatchAsync();
        }

        [Benchmark(Baseline = true)]
        public Task<List<Person>> EfCoreQueryBatchAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            return PersonDbContext.People.Take(BatchCount).ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> EfCoreQueryBatchNoChangeTrackingAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return PersonDbContext.People.Take(BatchCount).AsNoTracking().ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> EfCoreQueryBatchRawNoChangeTrackingAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return PersonDbContext.People.FromSqlRaw(sql).AsNoTracking().ToListAsync();
        }

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchAsync()
        {
            return Database.People.QueryBatch(sql).TrackChanges().Build().QueryAsync();
        }

        [Benchmark]
        public Task<List<Person>> VenflowQueryBatchNoChangeTrackingAsync()
        {
            return Database.People.QueryBatch(sql).Build().QueryAsync();
        }

        [Benchmark]
        public async Task<List<Person>> RepoDbQueryBatchAsync()
        {
            return EnumerableExtension.AsList(await DbConnectionExtension.QueryAsync<Person>(Database.GetConnection(), what: null, top: BatchCount));
        }

        [Benchmark]
        public async Task<List<Person>> DapperQueryBatchAsync()
        {
            return SqlMapper.AsList(await SqlMapper.QueryAsync<Person>(Database.GetConnection(), sql));
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}