using System;
using System.Data;
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

        protected VenflowBaseCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand)
        {
            Database = database;
            EntityConfiguration = entityConfiguration;
            UnderlyingCommand = underlyingCommand;
            DisposeCommand = disposeCommand;
        }

        protected ValueTask ValidateConnectionAsync()
        {
            if (UnderlyingCommand.Connection.State == ConnectionState.Open)
                return default;

            if (UnderlyingCommand.Connection.State == ConnectionState.Closed)
            {
                return new ValueTask(UnderlyingCommand.Connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{UnderlyingCommand.Connection.State}'.");
            }
        }
    }
}
