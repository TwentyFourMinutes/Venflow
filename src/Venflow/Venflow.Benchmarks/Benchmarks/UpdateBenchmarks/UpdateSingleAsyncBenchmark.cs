using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using Venflow.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks.UpdateBenchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RPlotExporter]
    public class UpdateSingleAsyncBenchmark : BenchmarkBase
    {
        private Person _efCorePerson;
        private Person _venflowPerson;
        private Person _repoDbPerson;

        private int index = 0;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            _efCorePerson = await PersonDbContext.People.FirstOrDefaultAsync();
            _venflowPerson = await Database.People.QuerySingle(@"SELECT * FROM ""People"" LIMIT 1").TrackChanges().Build().QueryAsync();
            _repoDbPerson = (await DbConnectionExtension.QueryAsync<Person>(Database.GetConnection(), whereOrPrimaryKey: null, top: 1)).FirstOrDefault();

            await EFCoreUpdateSingleAsync();
            await VenflowUpdateSingleAsync();
            await RepoDbUpdateSingleAsync();
        }

        [Benchmark(Baseline = true)]
        public Task EFCoreUpdateSingleAsync()
        {
            _efCorePerson.Name = "EFCoreUpdateSingleAsync" + index++.ToString();

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task VenflowUpdateSingleAsync()
        {
            _venflowPerson.Name = "VenflowUpdateSingleAsync" + index++.ToString();

            return Database.People.UpdateAsync(_venflowPerson);
        }

        [Benchmark]
        public Task RepoDbUpdateSingleAsync()
        {
            _repoDbPerson.Name = "RepoDbUpdateSingleAsync" + index++.ToString();

            return DbConnectionExtension.UpdateAsync(Database.GetConnection(), _repoDbPerson);
        }

        [GlobalCleanup]
        public override Task Cleanup()
        {
            return base.Cleanup();
        }
    }
}
