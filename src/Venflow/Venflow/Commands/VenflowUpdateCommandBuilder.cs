using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommandBuilder<TEntity> : IUpdateCommandBuilder<TEntity> where TEntity : class, new()
    {
        private readonly bool _disposeCommand;
        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowUpdateCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        IUpdateCommand<TEntity> ISpecficVenflowCommandBuilder<IUpdateCommand<TEntity>>.Build()
        {
            return new VenflowUpdateCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }
    }
}