using System.Collections.Generic;
using System.Reflection;
using Venflow.Modeling.Definitions;

namespace Venflow
{
    /// <summary>
    /// Provides an option builder to further configure a <see cref="Database"/> instance.
    /// </summary>
    public class DatabaseOptionsBuilder
    {
        private readonly List<Assembly> _configurationAssemblies;
        private readonly Assembly _databaseAssembly;

        internal DatabaseOptionsBuilder(Assembly databaseAssembly)
        {
            _configurationAssemblies = new List<Assembly>();
            _databaseAssembly = databaseAssembly;
        }

        /// <summary>
        /// Adds the assembly of the type <typeparamref name="T"/> to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <typeparam name="T">The type of which the assembly should be added to the lookup list.</typeparam>
        public void AddConfigurations<T>()
        {
            _configurationAssemblies.Add(typeof(T).Assembly);
        }

        /// <summary>
        /// Adds the assembly to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        ///<param name="assembly">The assembly which should be added to the lookup list.</param>
        public void AddConfigurations(Assembly assembly)
        {
            _configurationAssemblies.Add(assembly);
        }

        /// <summary>
        /// Adds the assemblies to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        ///<param name="assemblies">The assemblies which should be added to the lookup list.</param>
        public void AddConfigurations(params Assembly[] assemblies)
        {
            _configurationAssemblies.AddRange(assemblies);
        }

        internal DatabaseOptions Build()
        {
            if (_configurationAssemblies.Count == 0)
            {
                _configurationAssemblies.Add(_databaseAssembly);
            }

            return new DatabaseOptions(_configurationAssemblies);
        }
    }

    internal class DatabaseOptions
    {
        internal IReadOnlyList<Assembly> ConfigurationAssemblies { get; }

        internal DatabaseOptions(List<Assembly> configurationAssemblies)
        {
            ConfigurationAssemblies = configurationAssemblies;
        }
    }
}
