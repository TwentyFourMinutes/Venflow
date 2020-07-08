using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommand<TEntity> : VenflowBaseCommand<TEntity>, IInsertCommand<TEntity> where TEntity : class
    {
        internal Delegate? SingleInserter { get; set; }
        internal Delegate? BatchInserter { get; set; }

        private readonly bool _returnComputedColumns;

        internal VenflowInsertCommand(DbConfiguration dbConfiguration, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool returnComputedColumns, bool disposeCommand) : base(dbConfiguration, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _returnComputedColumns = returnComputedColumns;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, List<TEntity>, Task<int>> inserter;

            if (SingleInserter is { })
            {
                inserter = (Func<NpgsqlConnection, List<TEntity>, Task<int>>)SingleInserter;
            }
            else
            {
                SingleInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter(DbConfiguration);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, new List<TEntity> { entity });

            if (DisposeCommand)
                await this.DisposeAsync();

            return affectedRows;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, List<TEntity>, Task<int>> inserter;

            if (BatchInserter is { })
            {
                inserter = (Func<NpgsqlConnection, List<TEntity>, Task<int>>)BatchInserter;
            }
            else
            {
                BatchInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter(DbConfiguration);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entities);

            if (DisposeCommand)
                await this.DisposeAsync();

            return affectedRows;
        }

        public async ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            if (UnderlyingCommand.IsPrepared)
            {
                await UnderlyingCommand.UnprepareAsync();
            }
        }
    }
}
