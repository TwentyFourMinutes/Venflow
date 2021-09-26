using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Dynamic;
using Venflow.Dynamic.Proxies;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommand<TEntity> : VenflowBaseCommand<TEntity>, IUpdateCommand<TEntity> where TEntity : class, new()
    {
        private const int _minEntityStringLength = 35; // Rough estimate of minimum length

        internal VenflowUpdateCommand(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, new(), disposeCommand, loggers, shouldLog)
        {

        }

        ValueTask IUpdateCommand<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return default;

            var commandString = new StringBuilder(_minEntityStringLength);

            BaseUpdate(entity, 0, commandString);

            if (commandString.Length == 0)
            {
                return new ValueTask();
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask(ExecuteBase(CommandType.UpdateSingle, false, cancellationToken));
        }

        ValueTask IUpdateCommand<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null)
                return default;

            var commandString = new StringBuilder(_minEntityStringLength);

            var index = 0;

            foreach (var entity in entities)
            {
                BaseUpdate(entity, index++, commandString);
            }

            if (index == 0 ||
                commandString.Length == 0)
            {
                return new ValueTask();
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask(ExecuteBase(CommandType.UpdateBatch, index >= 10 && UnderlyingCommand.IsPrepared, cancellationToken));
        }

        ValueTask IUpdateCommand<TEntity>.UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return default;

            var commandString = new StringBuilder(entities.Count * _minEntityStringLength);

            for (int i = 0; i < entities.Count; i++)
            {
                BaseUpdate(entities[i], i, commandString);
            }

            if (commandString.Length == 0)
            {
                return new ValueTask();
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask(ExecuteBase(CommandType.UpdateBatch, entities.Count >= 10 && UnderlyingCommand.IsPrepared, cancellationToken));
        }

        private async Task ExecuteBase(Enums.CommandType commandType, bool shouldPrepare, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            if (shouldPrepare)
            {
                await UnderlyingCommand.PrepareAsync(cancellationToken);
            }

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                Log(commandType);
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(commandType, ex);
            }
            finally
            {
                if (ShouldAutoCommit)
                    await transaction.DisposeAsync();
                else
                    await transaction.ReleaseAsync(TransactionName, cancellationToken);

                if (shouldPrepare)
                {
                    await UnderlyingCommand.UnprepareAsync(cancellationToken);
                }

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

#if NET5_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
        private void BaseUpdate(TEntity entity, int index, StringBuilder commandString)
        {
            if (entity is not IEntityProxy<TEntity> proxy)
            {
                throw new InvalidOperationException("The provided entity is currently not being change tracked. Also ensure that the entity itself has properties which are marked as virtual.");
            }
            else if (!proxy.ChangeTracker.IsDirty)
            {
                return;
            }

            lock (proxy.ChangeTracker)
            {
                proxy.ChangeTracker.IsDirty = false;

                commandString.Append("UPDATE ")
                             .Append(EntityConfiguration.TableName)
                             .Append(" SET ");

                var columns = proxy.ChangeTracker.GetColumns().AsSpan();

                var entityColumns = EntityConfiguration.Columns.Values.AsSpan();

                for (int i = columns.Length - 1; i >= 0; i--)
                {
                    var columnIndex = columns[i];

                    if (columnIndex == 0)
                        continue;

                    var column = entityColumns[columnIndex - 1];

                    commandString.Append('"')
                                 .Append(column.ColumnName)
                                 .Append("\" = ");

                    var parameter = column.ValueRetriever(entity, index.ToString());

                    commandString.Append(parameter.ParameterName);

                    UnderlyingCommand.Parameters.Add(parameter);

                    commandString.Append(", ");
                }
            }

            commandString.Length -= 2;

            commandString.Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" = ");

            var primaryParameter = EntityConfiguration.PrimaryColumn.ValueRetriever(entity, "PK" + index);

            UnderlyingCommand.Parameters.Add(primaryParameter);

            commandString.Append(primaryParameter.ParameterName)
                         .Append(';');
        }

        public ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            return default;
        }
    }
}
