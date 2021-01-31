using System;
using Microsoft.Extensions.Configuration;

namespace Venflow.Shared
{
    public static class SecretsHandler
    {
        public static string GetConnectionString<T>(string type) where T : class
        {
            var configuration = new ConfigurationBuilder();

            if (IsDevelopmentMachine())
            {
                return configuration.AddUserSecrets<T>().Build().GetConnectionString(type);
            }
            else
            {
                return configuration.AddEnvironmentVariables().Build()["VENFLOW_TESTS_CONNECTION_STRING"];
            }
        }

        public static bool IsDevelopmentMachine()
            => Environment.GetEnvironmentVariable("VENFLOW_TESTS_CONNECTION_STRING") is null;
    }
}
