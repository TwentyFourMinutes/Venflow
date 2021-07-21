using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal abstract class VenflowBaseCommand<TEntity> where TEntity : class, new()
    {
        internal bool DisposeCommand { get; set; }
        internal Database Database { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; }
        internal NpgsqlCommand UnderlyingCommand { get; }

        protected bool ShouldAutoCommit = true;
        protected const string TransactionName = "_VenflowSavepoint";

        private readonly List<LoggerCallback> _loggers;
        private readonly bool _shouldLog;

        protected VenflowBaseCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, List<LoggerCallback> loggers, bool shouldLog)
        {
            Database = database;
            EntityConfiguration = entityConfiguration;
            UnderlyingCommand = underlyingCommand;
            DisposeCommand = disposeCommand;

            _loggers = loggers;
            _shouldLog = shouldLog;
        }

        protected bool Log(Enums.CommandType commandType, Exception? exception = default)
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

            if (VenflowConfiguration.ThrowLoggedExceptions &&
                exception is not null)
                throw exception;

            return true;
        }

        protected ValueTask ValidateConnectionAsync(bool hasGeneratedCommands = false)
        {
            var connection = hasGeneratedCommands ? Database.GetConnection() : UnderlyingCommand.Connection;

            if (connection.State == System.Data.ConnectionState.Open)
                return default;

            if (connection.State == System.Data.ConnectionState.Closed)
            {
                return new ValueTask(connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{System.Data.ConnectionState.Open}' or '{System.Data.ConnectionState.Closed}'. Actual: '{connection.State}'.");
            }
        }

        protected ValueTask<IDatabaseTransaction> GetTransactionAsync(
#if !NET48
            CancellationToken cancellationToken = default
#endif
            )
        {
            ShouldAutoCommit = !Database.HasActiveTransaction;

#if NET48
            return Database.GetOrCreateTransactionAsync();
#else
            return Database.GetOrCreateTransactionAsync(cancellationToken);
#endif
        }
    }
}
