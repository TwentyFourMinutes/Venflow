using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal abstract class VenflowBaseCommand<TEntity> where TEntity : class, new()
    {
        internal bool DisposeCommand { get; set; }

        internal Database Database { get; }
        internal Entity<TEntity> EntityConfiguration { get; }
        internal NpgsqlCommand UnderlyingCommand { get; }

        private readonly List<(Action<string> logger, bool includeSensitiveData)> _loggers;
        private readonly bool _shouldLog;

        protected VenflowBaseCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, List<(Action<string> logger, bool includeSensitiveData)> loggers, bool shouldLog)
        {
            Database = database;
            EntityConfiguration = entityConfiguration;
            UnderlyingCommand = underlyingCommand;
            DisposeCommand = disposeCommand;

            _loggers = loggers;
            _shouldLog = shouldLog;
        }

        protected void Log(Venflow.Enums.CommandType commandType, Exception? exception = default)
        {
            if (_shouldLog)
            {
                if (_loggers.Count == 0)
                {
                    Database.ExecuteLoggers(UnderlyingCommand, commandType, exception);
                }
                else
                {
                    Database.ExecuteLoggers(_loggers, UnderlyingCommand, commandType, exception);
                }
            }

            if (VenflowConfiguration.ThrowLoggedExceptions)
            {
                throw exception;
            }
        }

        protected ValueTask ValidateConnectionAsync()
        {
            if (UnderlyingCommand.Connection.State == System.Data.ConnectionState.Open)
                return default;

            if (UnderlyingCommand.Connection.State == System.Data.ConnectionState.Closed)
            {
                return new ValueTask(UnderlyingCommand.Connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{System.Data.ConnectionState.Open}' or '{System.Data.ConnectionState.Closed}'. Actual: '{UnderlyingCommand.Connection.State}'.");
            }
        }
    }
}
