using Venflow.Modeling;

namespace Venflow.Benchmarks.Benchmarks.Models.Configurations
{
    public class BenchmarkDbConfiguration : DbConfiguration
    {
        public Table<Person> People { get; set; }
        public Table<Email> Emails { get; set; }
        public Table<EmailContent> Contents { get; set; }

        public BenchmarkDbConfiguration() : base(SecretsHandler.GetConnectionString())
        {
        }
    }
}
