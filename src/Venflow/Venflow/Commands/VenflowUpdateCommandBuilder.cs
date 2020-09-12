using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommandBuilder<TEntity> : IUpdateCommandBuilder<TEntity> where TEntity : class, new()
    {
        private bool _disposeCommand;

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

        public IUpdateCommand<TEntity> Build()
        {
            return new VenflowUpdateCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        Task IUpdateCommandBuilder<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entity, cancellationToken);
        }

        Task IUpdateCommandBuilder<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {

            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }
    }
}