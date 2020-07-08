using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommandBuilder<TEntity> : IDeleteCommandBuilder<TEntity> where TEntity : class
    {
        private readonly bool _disposeCommand;
        private readonly NpgsqlCommand _command;
        private readonly DbConfiguration _dbConfiguration;
        private readonly Entity<TEntity> _entityConfiguration;

        internal VenflowDeleteCommandBuilder(DbConfiguration dbConfiguration, Entity<TEntity> entityConfiguration, NpgsqlCommand command, bool disposeCommand)
        {
            _dbConfiguration = dbConfiguration;
            _entityConfiguration = entityConfiguration;
            _command = command;
            _disposeCommand = disposeCommand;
        }

        IDeleteCommand<TEntity> ISpecficVenflowCommandBuilder<IDeleteCommand<TEntity>>.Build()
        {
            return new VenflowDeleteCommand<TEntity>(_dbConfiguration, _entityConfiguration, _command, _disposeCommand);
        }
    }
}