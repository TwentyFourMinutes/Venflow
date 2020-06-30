using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Benchmarks.Benchmarks.Models.Configurations;

namespace Venflow.Benchmarks.Benchmarks
{
    public abstract class BenchmarkBase
    {
        public BenchmarkDbConfiguration Configuration { get; set; }
        public VenflowDbConnection VenflowDbConnection { get; set; }
        public BenchmarkDbContext PersonDbContext { get; set; }

        public virtual async Task Setup()
        {
            Configuration = new BenchmarkDbConfiguration();

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

            VenflowDbConnection = await Configuration.NewConnectionScopeAsync();
        }

        public virtual async Task Cleanup()
        {
            await VenflowDbConnection.DisposeAsync();

            await PersonDbContext.DisposeAsync();
        }
    }
}
