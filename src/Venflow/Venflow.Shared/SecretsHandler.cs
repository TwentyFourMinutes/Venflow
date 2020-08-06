using Microsoft.Extensions.Configuration;

namespace Venflow.Shared
{
    public static class SecretsHandler
    {
        public static string GetConnectionString<T>() where T : class
        {
            var configuration = new ConfigurationBuilder().AddUserSecrets<T>();

            var config = configuration.Build();

            return config.GetConnectionString("PostgreSQL");
        }
    }
}
