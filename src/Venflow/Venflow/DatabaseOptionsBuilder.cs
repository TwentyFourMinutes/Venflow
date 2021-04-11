using System;
using System.Collections.Generic;
using System.Reflection;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling.Definitions;

namespace Venflow
{
    public delegate void LoggerCallback(NpgsqlCommand command, CommandType commandType, Exception? exception);

    /// <summary>
    /// Provides an option builder to further configure a <see cref="Database"/> instance.
    /// </summary>
    public class DatabaseOptionsBuilder
    {
        /// <summary>
        /// Gets or sets the default LoggingBehavior on commands for this <see cref="Database"/>. The default is <see cref="LoggingBehavior.Always"/>, if any loggers are defined.
        /// </summary>
        public LoggingBehavior DefaultLoggingBehavior { get; set; }

        internal Type EffectiveDatabaseType { get; }

        private readonly List<Assembly> _configurationAssemblies;
        private readonly List<LoggerCallback> _loggers;
        private readonly Assembly _databaseAssembly;

        internal DatabaseOptionsBuilder(Type effectiveDatabaseType)
        {
            _configurationAssemblies = new(1);
            _loggers = new(0);

            _databaseAssembly = effectiveDatabaseType.Assembly;
            EffectiveDatabaseType = effectiveDatabaseType;
        }

        /// <summary>
        /// Adds the assembly of the type <typeparamref name="T"/> to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <typeparam name="T">The type of which the assembly should be added to the lookup list.</typeparam>
        /// <returns>An object that can be used to configure the current <see cref="Database"/>.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseOptionsBuilder UseConfigurations<T>()
        {
            _configurationAssemblies.Add(typeof(T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the assembly to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assembly">The assembly which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/>.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseOptionsBuilder UseConfigurations(Assembly assembly)
        {
            _configurationAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Adds the assemblies to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assemblies">The assemblies which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/>.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseOptionsBuilder UseConfigurations(params Assembly[] assemblies)
        {
            _configurationAssemblies.AddRange(assemblies);

            return this;
        }

        /// <summary>
        /// Adds a logger, which allows for logging of executed commands.
        /// </summary>
        /// <param name="loggerCallback">A callback which is being used to log commands.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/>.</returns>
        /// <remarks>This currently only includes the following API's:
        /// <list type="bullet">
        ///     <item>QuerySingle</item>
        ///     <item>QueryBatch</item>
        ///     <item>QueryAsync</item>
        /// </list>
        /// Also consider configuring the <see cref="DefaultLoggingBehavior"/> property.
        /// </remarks>
        public DatabaseOptionsBuilder LogTo(LoggerCallback loggerCallback)
        {
            _loggers.Add(loggerCallback);

            return this;
        }

        internal DatabaseOptions Build()
        {
            if (_configurationAssemblies.Count == 0)
            {
                _configurationAssemblies.Add(_databaseAssembly);
            }

            return new DatabaseOptions(_configurationAssemblies, _loggers, _loggers.Count == 0 ? LoggingBehavior.Never : DefaultLoggingBehavior);
        }
    }

    internal class DatabaseOptions
    {
        internal IReadOnlyList<Assembly> ConfigurationAssemblies { get; }
        internal IReadOnlyList<LoggerCallback> Loggers { get; }

        internal LoggingBehavior DefaultLoggingBehavior { get; }

        internal DatabaseOptions(List<Assembly> configurationAssemblies, IReadOnlyList<LoggerCallback> loggers, LoggingBehavior defaultLoggingBehavior)
        {
            ConfigurationAssemblies = configurationAssemblies;
            Loggers = loggers;
            DefaultLoggingBehavior = defaultLoggingBehavior;
        }
    }
}
