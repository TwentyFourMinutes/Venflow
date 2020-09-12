using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommand<TEntity> : VenflowBaseCommand<TEntity>, IInsertCommand<TEntity> where TEntity : class, new()
    {
        internal Delegate? SingleInserter { get; set; }
        internal Delegate? BatchInserter { get; set; }

        private readonly InsertOptions _insertOptions;

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, InsertOptions insertOptions, bool disposeCommand) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _insertOptions = insertOptions;
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
                SingleInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<TEntity>(_insertOptions);
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
                BatchInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<IList<TEntity>>(_insertOptions);
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
