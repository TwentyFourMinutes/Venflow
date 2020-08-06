using Microsoft.EntityFrameworkCore;
using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Models;
using Venflow.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    public abstract class BenchmarkBase
    {
        public BenchmarkDb Database { get; set; }
        public BenchmarkDbContext PersonDbContext { get; set; }

        public virtual async Task Setup()
        {
            Database = new BenchmarkDb();

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

            PersonDbContext = new BenchmarkDbContext();

            PersonDbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            PersonDbContext.ChangeTracker.LazyLoadingEnabled = false;
            PersonDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public virtual async Task Cleanup()
        {
            await Database.DisposeAsync();

            await PersonDbContext.DisposeAsync();
        }
    }
}
