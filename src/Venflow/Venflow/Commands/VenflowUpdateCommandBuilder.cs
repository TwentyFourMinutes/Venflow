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

        internal VenflowUpdateCommandBuilder(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;

            _command = new();
        }

        public IUpdateCommand<TEntity> Build()
        {
            return new VenflowUpdateCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entity, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }
    }
}