using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using Venflow.Benchmarks.Models;
using Venflow.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    public abstract class BenchmarkBase
    {
        public BenchmarkDb Database { get; set; } = null!;
        public BenchmarkDbContext PersonDbContext { get; set; } = null!;

        private static bool _initDone = false;

        public virtual Task Setup()
        {
            Database = new BenchmarkDb();

            BenchmarkHandler.Init(Database);

            if (!_initDone)
            {
                PostgreSqlBootstrap.Initialize();
                ClassMapper.Add<Person>("\"People\"");
                ClassMapper.Add<Email>("\"Emails\"");
                ClassMapper.Add<EmailContent>("\"EmailContents\"");
                IdentityMapper.Add<Person>(x => x.Id);
                IdentityMapper.Add<Email>(x => x.Id);
                IdentityMapper.Add<EmailContent>(x => x.Id);
                PrimaryMapper.Add<Person>(x => x.Id);
                PrimaryMapper.Add<Email>(x => x.Id);
                PrimaryMapper.Add<EmailContent>(x => x.Id);

                _initDone = true;
            }

            PersonDbContext = new BenchmarkDbContext();

            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.LazyLoadingEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return Task.CompletedTask;
        }

        public virtual async Task Cleanup()
        {
            await Database.People.TruncateAsync(Enums.ForeignTruncateOptions.Cascade);

            await Database.DisposeAsync();

            await PersonDbContext.DisposeAsync();
        }
    }
}
