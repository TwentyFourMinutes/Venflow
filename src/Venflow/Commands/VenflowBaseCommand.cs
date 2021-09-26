using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal abstract class VenflowBaseCommand<TEntity> where TEntity : class, new()
    {
        internal bool DisposeCommand { get; set; }
        internal Database Database { get; set; }
        internal NpgsqlCommand UnderlyingCommand { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; }

        protected List<LoggerCallback> Loggers { get; }
        protected bool ShouldLog => _shouldLog && (Database.HasLoggers || Loggers.Count > 0);

        protected bool ShouldAutoCommit = true;
        protected const string TransactionName = "_VenflowSavepoint";
        private readonly bool _shouldLog;

        protected VenflowBaseCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand? underlyingCommand, bool disposeCommand, List<LoggerCallback> loggers, bool shouldLog)
        {
            Database = database;
            EntityConfiguration = entityConfiguration;
            UnderlyingCommand = underlyingCommand!;
            DisposeCommand = disposeCommand;

            Loggers = loggers;
            _shouldLog = shouldLog;

            if (underlyingCommand is not null)
                underlyingCommand.Connection = database.GetConnection();
        }

        protected bool Log(Enums.CommandType commandType, Exception? exception = default)
        {
            if (ShouldLog)
            {
                if (Loggers.Count == 0)
                {
                    Database.ExecuteLoggers(UnderlyingCommand, commandType, exception);
                }
                else
                {
                    Database.ExecuteLoggers(Loggers, UnderlyingCommand, commandType, exception);
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

            if (connection!.State == System.Data.ConnectionState.Open)
                return default;

            if (connection!.State == System.Data.ConnectionState.Closed)
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
