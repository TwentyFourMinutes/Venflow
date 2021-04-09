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

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, bool isFullInsert, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand, loggers, shouldLog)
        {
            _isFullInsert = isFullInsert;

            underlyingCommand.Connection = database.GetConnection();
        }

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, RelationBuilderValues? relationBuilderValues, bool isFullInsert, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand, loggers, shouldLog)
        {
            _relationBuilderValues = relationBuilderValues;
            _isFullInsert = isFullInsert;

            underlyingCommand.Connection = database.GetConnection();
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, TEntity, CancellationToken, Task<int>> inserter;

            if (SingleInserter is not null)
            {
                inserter = (SingleInserter as Func<NpgsqlConnection, TEntity, CancellationToken, Task<int>>)!;
            }
            else
            {
                SingleInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<TEntity>(_relationBuilderValues, true, _isFullInsert);
            }

            var transaction = await Database.BeginTransactionAsync(
#if NET5_0_OR_GREATER
                cancellationToken
#endif
            );

            try
            {
                var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entity, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
            finally
            {
                await transaction.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            Func<NpgsqlConnection, IList<TEntity>, CancellationToken, Task<int>> inserter;

            if (BatchInserter is not null)
            {
                inserter = (Func<NpgsqlConnection, IList<TEntity>, CancellationToken, Task<int>>)BatchInserter;
            }
            else
            {
                BatchInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<IList<TEntity>>(_relationBuilderValues, false, _isFullInsert);
            }

            var transaction = await Database.BeginTransactionAsync(
#if NET5_0_OR_GREATER
                cancellationToken
#endif
            );

            try
            {
                var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entities, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
            finally
            {
                await transaction.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

        public ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            return new ValueTask();
        }
    }
}
