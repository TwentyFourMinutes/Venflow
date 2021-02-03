using System;
using Microsoft.Extensions.Configuration;

namespace Venflow.Shared
{
    public static class SecretsHandler
    {
        public static string GetConnectionString<T>(string type) where T : class
        {
            var configuration = new ConfigurationBuilder();

            if (IsDevelopmentMachine(type))
            {
                return configuration.AddUserSecrets<T>().Build().GetConnectionString(type);
            }
            else
            {
                return configuration.AddEnvironmentVariables().Build()[$"VENFLOW_{type.ToUpper()}_CONNECTION_STRING"];
            }
        }

        public static bool IsDevelopmentMachine(string type)
            => Environment.GetEnvironmentVariable($"VENFLOW_{type.ToUpper()}_CONNECTION_STRING") is null;
    }
}