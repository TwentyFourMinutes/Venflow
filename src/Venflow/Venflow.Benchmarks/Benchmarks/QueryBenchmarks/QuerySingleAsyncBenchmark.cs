using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using Venflow.Benchmarks.Benchmarks.InsertBenchmarks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.QueryBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class QuerySingleAsyncBenchmark : BenchmarkBase
    {
        private const string sql = @"SELECT * FROM ""People"" LIMIT 1";

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            var insertBenchmark = new InsertBatchWithRelationsAsyncBenchmark();

            await insertBenchmark.Setup();

            insertBenchmark.InsertCount = 10000;

            await insertBenchmark.VenflowInsertBatchAsync();

            await insertBenchmark.Database.DisposeAsync();

            await insertBenchmark.PersonDbContext.DisposeAsync();

            await EfCoreQuerySingleAsync();
            await EfCoreQuerySingleNoChangeTrackingAsync();
            await EfCoreQuerySingleRawNoChangeTrackingAsync();
            await VenflowQuerySingleAsync();
            await VenflowQuerySingleNoChangeTrackingAsync();
            await RepoDbQuerySingleAsync();
            await DapperQuerySingleAsync();
        }

        [Benchmark(Baseline = true)]
        public Task<Person> EfCoreQuerySingleAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            return PersonDbContext.People.FirstOrDefaultAsync();
        }

        [Benchmark]
        public Task<Person> EfCoreQuerySingleNoChangeTrackingAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return PersonDbContext.People.AsNoTracking().FirstOrDefaultAsync();
        }

        [Benchmark]
        public Task<Person> EfCoreQuerySingleRawNoChangeTrackingAsync()
        {
            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return PersonDbContext.People.FromSqlRaw(sql).AsNoTracking().FirstOrDefaultAsync();
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleAsync()
        {
            return Database.People.QuerySingle(sql).TrackChanges().Build().QueryAsync();
        }

        [Benchmark]
        public Task<Person> VenflowQuerySingleNoChangeTrackingAsync()
        {
            return Database.People.QuerySingle(sql).Build().QueryAsync();
        }

        [Benchmark]
        public Task<Person> RepoDbQuerySingleAsync()
        {
            return DbConnectionExtension.QueryAsync<Person>(Database.GetConnection(), what: null, top: 1).ContinueWith(x => x.Result.First());
        }

        [Benchmark]
        public Task<Person> DapperQuerySingleAsync()
        {
            return SqlMapper.QueryFirstAsync<Person>(Database.GetConnection(), sql);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
