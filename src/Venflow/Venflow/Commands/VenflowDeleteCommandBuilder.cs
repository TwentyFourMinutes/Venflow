using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommandBuilder<TEntity> : IDeleteCommandBuilder<TEntity> where TEntity : class, new()
    {
        private bool _disposeCommand;

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

        public IDeleteCommand<TEntity> Build()
        {
            return new VenflowDeleteCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entity, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }
    }
}