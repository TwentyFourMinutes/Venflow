using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Venflow.Json;

namespace Venflow.AspNetCore
{
    public static class VenflowServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            return AddDatabase(services, optionsAction is null ? null : (Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>)((_, options) => optionsAction.Invoke(options)), databaseLifetime, databaseLifetime);
        }

        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            return AddDatabase(services, optionsAction, databaseLifetime, databaseLifetime);
        }

        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            services.TryAdd(new ServiceDescriptor(typeof(TDatabase), typeof(TDatabase), databaseLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(DatabaseOptionsBuilder<TDatabase>), x => GetDatbaseOptionsBuilder(x, optionsAction), optionsLifetime));

            services.Add(new ServiceDescriptor(typeof(DatabaseOptionsBuilder), x => x.GetRequiredService<DatabaseOptionsBuilder<TDatabase>>(), optionsLifetime));

            return services;
        }

        public static IServiceCollection AddVenflowJson(this IServiceCollection services)
        {
            services.AddOptions<JsonOptions>().Configure(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonKeyConverterFactory());
            });

            return services;
        }

        private static DatabaseOptionsBuilder<TDatabase> GetDatbaseOptionsBuilder<TDatabase>(IServiceProvider serviceProvider, Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>? optionsAction)
            where TDatabase : Database
        {
            var options = new DatabaseOptionsBuilder<TDatabase>();

            optionsAction?.Invoke(serviceProvider, options);

            return options;
        }
    }
}
