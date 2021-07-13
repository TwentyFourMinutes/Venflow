using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using Venflow.Benchmarks.Benchmarks.InsertBenchmarks;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.UpdateBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]

    public class UpdateBatchAsyncBenchmark : BenchmarkBase
    {
        [Params(10, 100, 1000, 10000)]
        public int BatchCount { get; set; }

        private List<Person> _efCorePeople;
        private List<Person> _venflowPeople;
        private List<Person> _repoDbPeople;

        private int index = 0;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            var insertBenchmark = new InsertBatchWithRelationsAsyncBenchmark();

            await insertBenchmark.Setup();

            insertBenchmark.BatchCount = 10000;

            await insertBenchmark.VenflowInsertBatchAsync();

            await insertBenchmark.Database.DisposeAsync();

            await insertBenchmark.PersonDbContext.DisposeAsync();

            _efCorePeople = await PersonDbContext.People.Take(BatchCount).ToListAsync();
            _venflowPeople = await Database.People.QueryBatch(@"SELECT * FROM ""People"" LIMIT " + BatchCount).TrackChanges().Build().QueryAsync();
            _repoDbPeople = (await DbConnectionExtension.QueryAsync<Person>(Database.GetConnection(), what: null, top: BatchCount)).ToList();

            await EFCoreUpdateBatchAsync();
            await VenflowUpdateBatchAsync();
            await RepoDbUpdateBatchAsync();
        }

        [Benchmark(Baseline = true)]
        public Task EFCoreUpdateBatchAsync()
        {
            for (int i = 0; i < _efCorePeople.Count; i++)
            {
                _efCorePeople[i].Name = "EFCoreUpdateBatchAsync" + index++.ToString();
            }

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public ValueTask VenflowUpdateBatchAsync()
        {
            for (int i = 0; i < _venflowPeople.Count; i++)
            {
                _venflowPeople[i].Name = "VenflowUpdateBatchAsync" + index++.ToString();
            }

            return Database.People.UpdateAsync(_venflowPeople);
        }

        [Benchmark]
        public Task RepoDbUpdateBatchAsync()
        {
            for (int i = 0; i < _repoDbPeople.Count; i++)
            {
                _repoDbPeople[i].Name = "RepoDbUpdateSingleAsync" + index++.ToString();
            }

            return DbConnectionExtension.UpdateAllAsync(Database.GetConnection(), _repoDbPeople);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
