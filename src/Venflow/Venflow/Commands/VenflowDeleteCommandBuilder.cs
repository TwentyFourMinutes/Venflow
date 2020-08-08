using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommandBuilder<TEntity> : IDeleteCommandBuilder<TEntity> where TEntity : class, new()
    {
        private readonly bool _disposeCommand;
        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowDeleteCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        IDeleteCommand<TEntity> ISpecficVenflowCommandBuilder<IDeleteCommand<TEntity>>.Build()
        {
            return new VenflowDeleteCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }
    }
}