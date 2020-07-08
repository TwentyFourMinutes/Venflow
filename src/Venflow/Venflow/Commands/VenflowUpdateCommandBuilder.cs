using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommandBuilder<TEntity> : IUpdateCommandBuilder<TEntity> where TEntity : class
    {
        private readonly bool _disposeCommand;
        private readonly NpgsqlCommand _command;
        private readonly DbConfiguration _dbConfiguration;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowUpdateCommandBuilder(DbConfiguration dbConfiguration, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _dbConfiguration = dbConfiguration;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        IUpdateCommand<TEntity> ISpecficVenflowCommandBuilder<IUpdateCommand<TEntity>>.Build()
        {
            return new VenflowUpdateCommand<TEntity>(_dbConfiguration, _entityConfiguration, _command, _disposeCommand);
        }
    }
}