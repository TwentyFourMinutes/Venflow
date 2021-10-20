using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Venflow.Json;

namespace Venflow.AspNetCore
{
    /// <summary>
    /// Provides a set of useful extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class VenflowServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <typeparamref name="TDatabase"/> as a service in the <paramref name="services"/>.
        /// </summary>
        /// <typeparam name="TDatabase">The type of the database to be registered.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the database will be registered.</param>
        /// <param name="optionsAction">Allows to configure a <see cref="DatabaseOptionsBuilder{TDatabase}"/> for the database. Note, that the specified <typeparamref name="TDatabase"/> needs to have a public constructor with a <see cref="DatabaseOptionsBuilder{TDatabase}"/> parameter, which it passes to the base constructor.</param>
        /// <param name="databaseLifetime">The liftetime with which to register the <typeparamref name="TDatabase"/> service in the container.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            return AddDatabase(services, optionsAction is null ? null : (Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>)((_, options) => optionsAction.Invoke(options)), databaseLifetime, databaseLifetime);
        }

        /// <summary>
        /// Registers the <typeparamref name="TDatabase"/> as a service in the <paramref name="services"/>.
        /// </summary>
        /// <typeparam name="TDatabase">The type of the database to be registered.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the database will be registered.</param>
        /// <param name="optionsAction">Allows to configure a <see cref="DatabaseOptionsBuilder{TDatabase}"/> for the database. Note, that the specified <typeparamref name="TDatabase"/> needs to have a public constructor with a <see cref="DatabaseOptionsBuilder{TDatabase}"/> parameter, which it passes to the base constructor.</param>
        /// <param name="databaseLifetime">The liftetime with which to register the <typeparamref name="TDatabase"/> service in the container.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            return AddDatabase(services, optionsAction, databaseLifetime, databaseLifetime);
        }

        /// <summary>
        /// Registers the <typeparamref name="TDatabase"/> as a service in the <paramref name="services"/>.
        /// </summary>
        /// <typeparam name="TDatabase">The type of the database to be registered.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the database will be registered.</param>
        /// <param name="optionsAction">Allows to configure a <see cref="DatabaseOptionsBuilder{TDatabase}"/> for the database. Note, that the specified <typeparamref name="TDatabase"/> needs to have a public constructor with a <see cref="DatabaseOptionsBuilder{TDatabase}"/> parameter, which it passes to the base constructor.</param>
        /// <param name="databaseLifetime">The liftetime with which to register the <typeparamref name="TDatabase"/> service in the container.</param>
        /// <param name="optionsLifetime">The liftetime with which to register the <see cref="DatabaseOptionsBuilder{TDatabase}"/> service in the container.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddDatabase<TDatabase>(this IServiceCollection services, Action<IServiceProvider, DatabaseOptionsBuilder<TDatabase>>? optionsAction = null, ServiceLifetime databaseLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDatabase : Database
        {
            services.TryAdd(new ServiceDescriptor(typeof(TDatabase), typeof(TDatabase), databaseLifetime));

            services.TryAdd(new ServiceDescriptor(typeof(DatabaseOptionsBuilder<TDatabase>), x => GetDatbaseOptionsBuilder(x, optionsAction), optionsLifetime));

            services.Add(new ServiceDescriptor(typeof(DatabaseOptionsBuilder), x => x.GetRequiredService<DatabaseOptionsBuilder<TDatabase>>(), optionsLifetime));

            return services;
        }

        /// <summary>
        /// Adds support for Venflows build in strongly-typed id to System.Text.Json.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the handler will be registered.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        /// <remarks>
        /// If you are using Newtonsoft.Json, please use Venflow.NewtonsoftJson
        /// </remarks>
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
