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
        private Delegate? _singleInserter;
        private Delegate? _singleLoggingInserter;
        private Delegate? _batchInserter;
        private Delegate? _batchLoggingInserter;

        private readonly RelationBuilderValues? _relationBuilderValues;
        private readonly bool _isFullInsert;

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, bool isFullInsert, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, null, true, loggers, shouldLog)
        {
            _isFullInsert = isFullInsert;
        }

        internal VenflowInsertCommand(Database database, Entity<TEntity> entityConfiguration, RelationBuilderValues? relationBuilderValues, bool isFullInsert, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, null, true, loggers, shouldLog)
        {
            _relationBuilderValues = relationBuilderValues;
            _isFullInsert = isFullInsert;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync(true);

            Delegate inserter;

            if (ShouldLog &&
                _singleLoggingInserter is not null)
            {
                inserter = _singleLoggingInserter;
            }
            else if (!ShouldLog &&
                     _singleInserter is not null)
            {
                inserter = _singleInserter;
            }
            else
            {
                _singleInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<TEntity>(_relationBuilderValues, ShouldLog, true, _isFullInsert);
            }

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            UnderlyingCommand = new NpgsqlCommand
            {
                Connection = Database.GetConnection()
            };

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                int affectedRows;

                if (ShouldLog)
                {
                    affectedRows = await (inserter as Func<NpgsqlCommand, TEntity, Action<CommandType>, CancellationToken, Task<int>>)!.Invoke(UnderlyingCommand, entity, Log, cancellationToken);
                }
                else
                {
                    affectedRows = await (inserter as Func<NpgsqlCommand, TEntity, CancellationToken, Task<int>>)!.Invoke(UnderlyingCommand, entity, cancellationToken);
                }

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(CommandType.InsertSingle, ex);

                return default;
            }
            finally
            {
                if (ShouldAutoCommit)
                    await transaction.DisposeAsync();
                else
                    await transaction.ReleaseAsync(TransactionName, cancellationToken);

                UnderlyingCommand.Dispose();
            }
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync(true);

            Delegate inserter;

            if (ShouldLog &&
                _batchLoggingInserter is not null)
            {
                inserter = _batchLoggingInserter;
            }
            else if (!ShouldLog &&
                     _batchInserter is not null)
            {
                inserter = _batchInserter;
            }
            else
            {
                _batchInserter = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter<IList<TEntity>>(_relationBuilderValues, ShouldLog, false, _isFullInsert);
            }

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            UnderlyingCommand = new NpgsqlCommand
            {
                Connection = Database.GetConnection()
            };

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                int affectedRows;

                if (ShouldLog)
                {
                    affectedRows = await (inserter as Func<NpgsqlCommand, IList<TEntity>, Action<CommandType>, CancellationToken, Task<int>>)!.Invoke(UnderlyingCommand, entities, Log, cancellationToken);
                }
                else
                {
                    affectedRows = await (inserter as Func<NpgsqlCommand, IList<TEntity>, CancellationToken, Task<int>>)!.Invoke(UnderlyingCommand, entities, cancellationToken);
                }

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(CommandType.InsertBatch, ex);

                return default;
            }
            finally
            {
                if (ShouldAutoCommit)
                    await transaction.DisposeAsync();
                else
                    await transaction.ReleaseAsync(TransactionName, cancellationToken);

                UnderlyingCommand.Dispose();
            }
        }

        private void Log(CommandType commandType)
        {
            if (Loggers.Count == 0)
            {
                Database.ExecuteLoggers(UnderlyingCommand, commandType, null);
            }
            else
            {
                Database.ExecuteLoggers(Loggers, UnderlyingCommand, commandType, null);
            }
        }

        public ValueTask DisposeAsync()
            => default;
    }
}
