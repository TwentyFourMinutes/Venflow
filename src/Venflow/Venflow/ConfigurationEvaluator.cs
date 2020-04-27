using System;

namespace Venflow
{
    public static class ConfigurationEvaluator
    {
        public static void EvaluateConfiguration<TConfiguration>(string connectionString, bool useLazyEntityEvaluation = false) where TConfiguration : DbConfiguration
        {
            var configuration = (TConfiguration?)Activator.CreateInstance(typeof(TConfiguration), connectionString, useLazyEntityEvaluation);

            if (configuration is null)
            {
                throw new TypeArgumentException("Couldn't create an instance of the provided generic type argument.", nameof(TConfiguration));
            }

            configuration.Build();
        }
    }
}
