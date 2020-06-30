using Venflow.Modeling;

namespace Venflow.Benchmarks.Benchmarks.Models.Configurations
{
    public class BenchmarkDbConfiguration: DbConfiguration
    {
        public BenchmarkDbConfiguration() : base(SecretsHandler.GetConnectionString())
        {
        }

        protected override void Configure(DbConfigurator dbConfigurator)
        {
            dbConfigurator.AddEntity<PersonConfiguration, Person>();
            dbConfigurator.AddEntity<EmailConfiguration, Email>();
            dbConfigurator.AddEntity<EmailContentConfiguration, EmailContent>();
        }
    }
}
