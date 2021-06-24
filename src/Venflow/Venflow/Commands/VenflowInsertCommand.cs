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

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entity, cancellationToken);

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                Log(Enums.CommandType.InsertSingle);

                return affectedRows;
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(Enums.CommandType.InsertSingle, ex);

                return default;
            }
            finally
            {
                if (ShouldAutoCommit)
                    await transaction.DisposeAsync();
                else
                    await transaction.ReleaseAsync(TransactionName, cancellationToken);

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

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entities, cancellationToken);

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                Log(Enums.CommandType.InsertBatch);

                return affectedRows;
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(Enums.CommandType.InsertBatch, ex);

                return default;
            }
            finally
            {
                if (ShouldAutoCommit)
                    await transaction.DisposeAsync();
                else
                    await transaction.ReleaseAsync(TransactionName, cancellationToken);

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