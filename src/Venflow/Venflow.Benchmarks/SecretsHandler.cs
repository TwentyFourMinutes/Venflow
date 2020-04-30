using Microsoft.Extensions.Configuration;

namespace Venflow.Benchmarks
{
    public static class SecretsHandler
    {
        public static string GetConnectionString()
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<Startup>();

            var config = configuration.Build();

            return config.GetConnectionString("PostgreSQL");
        }
    }
}
