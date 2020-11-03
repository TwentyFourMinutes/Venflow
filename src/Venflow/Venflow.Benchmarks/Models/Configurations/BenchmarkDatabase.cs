using Venflow.Shared;

namespace Venflow.Benchmarks.Models.Configurations
{
    public class BenchmarkDatabase : Database
    {
        public Table<Person> People { get; set; }
        public Table<Email> Emails { get; set; }
        public Table<EmailContent> EmailContents { get; set; }

        public BenchmarkDatabase() : base(SecretsHandler.GetConnectionString<Startup>())
        {
        }
    }
}
