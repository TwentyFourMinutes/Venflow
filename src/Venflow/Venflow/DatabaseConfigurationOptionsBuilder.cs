using System;
using System.Collections.Generic;
using System.Reflection;
using Venflow.Modeling.Definitions;

namespace Venflow
{
    /// <summary>
    /// Provides an option builder to further <i>statically</i> configure a <see cref="Database"/> instance.
    /// </summary>
    public class DatabaseConfigurationOptionsBuilder
    {
        internal Type EffectiveDatabaseType { get; }

        internal List<Assembly> ConfigurationAssemblies { get; }

        internal DatabaseConfigurationOptionsBuilder(Type effectiveDatabaseType)
        {
            ConfigurationAssemblies = new(1);

            EffectiveDatabaseType = effectiveDatabaseType;
        }

        /// <summary>
        /// Adds the assembly of the type <typeparamref name="T"/> to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <typeparam name="T">The type of which the assembly should be added to the lookup list.</typeparam>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations<T>()
        {
            ConfigurationAssemblies.Add(typeof(T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the assembly to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assembly">The assembly which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations(Assembly assembly)
        {
            ConfigurationAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Adds the assemblies to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assemblies">The assemblies which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations(params Assembly[] assemblies)
        {
            ConfigurationAssemblies.AddRange(assemblies);

            return this;
        }
    }
}
