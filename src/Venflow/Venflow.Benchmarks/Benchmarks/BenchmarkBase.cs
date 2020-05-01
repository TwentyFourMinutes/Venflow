using RepoDb;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;

namespace Venflow.Benchmarks.Benchmarks
{
    public abstract class BenchmarkBase
    {
        public MyDbConfiguration Configuration { get; set; }
        public VenflowDbConnection VenflowDbConnection { get; set; }

        public virtual async Task Setup()
        {
            Configuration = new MyDbConfiguration();

            PostgreSqlBootstrap.Initialize();
            ClassMapper.Add<Person>("\"Persons\"");

            VenflowDbConnection = await Configuration.NewConnectionScopeAsync();
        }

        public virtual async Task Cleanup()
        {
            Configuration = new MyDbConfiguration();

            await VenflowDbConnection.DisposeAsync();
        }
    }
}
