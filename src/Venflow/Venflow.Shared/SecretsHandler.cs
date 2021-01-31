using System;
using Microsoft.Extensions.Configuration;

namespace Venflow.Shared
{
    public static class SecretsHandler
    {
        public static string GetConnectionString<T>() where T : class
        {
            var configuration = new ConfigurationBuilder();

            if (Environment.GetEnvironmentVariable("venflow-tests-connection-string") is null)
            {
                return configuration.AddUserSecrets<T>().Build().GetConnectionString("PostgreSQL");
            }
            else
            {
                return configuration.AddEnvironmentVariables().Build()["venflow-tests-connection-string"];
            }
        }
    }
}
