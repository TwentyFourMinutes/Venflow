using System.Collections.Generic;
using System.Reflection;

namespace Venflow
{
    public class DatabaseOptionsBuilder
    {
        private readonly List<Assembly> _configurationAssemblies;
        private readonly Assembly _databaseAssembly;

        internal DatabaseOptionsBuilder(Assembly databaseAssembly)
        {
            _configurationAssemblies = new List<Assembly>();
            _databaseAssembly = databaseAssembly;
        }

        public void AddConfigurations<T>()
        {
            _configurationAssemblies.Add(typeof(T).Assembly);
        }

        public void AddConfigurations(Assembly assembly)
        {
            _configurationAssemblies.Add(assembly);
        }

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
            ConfigurationAssemblies = configurationAssemblies.AsReadOnly();
        }
    }
}
