using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommand<TEntity> : VenflowBaseCommand<TEntity>, IInsertCommand<TEntity> where TEntity : class, new()
    {
        internal Delegate? SingleInserter { get; set; }
        internal Delegate? BatchInserter { get; set; }

        private readonly RelationBuilderValues? _relationBuilderValues;
        private readonly bool _isFullInsert;

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, bool isFullInsert) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _isFullInsert = isFullInsert;
        }

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, RelationBuilderValues? relationBuilderValues, bool isFullInsert) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _relationBuilderValues = relationBuilderValues;
            _isFullInsert = isFullInsert;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, TEntity, CancellationToken, Task<int>> inserter;

            if (SingleInserter is { })
            {
                inserter = (Func<NpgsqlConnection, TEntity, CancellationToken, Task<int>>)SingleInserter;
            }
            else
            {
                SingleInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<TEntity>(_relationBuilderValues, true, _isFullInsert);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entity, cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();

            return affectedRows;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, IList<TEntity>, CancellationToken, Task<int>> inserter;

            if (BatchInserter is { })
            {
                inserter = (Func<NpgsqlConnection, IList<TEntity>, CancellationToken, Task<int>>)BatchInserter;
            }
            else
            {
                BatchInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<IList<TEntity>>(_relationBuilderValues, false, _isFullInsert);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entities, cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();

            return affectedRows;
        }

        public ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            return new ValueTask();
        }
    }
}
