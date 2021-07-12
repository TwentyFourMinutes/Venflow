﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommand<TEntity> : VenflowBaseCommand<TEntity>, IDeleteCommand<TEntity> where TEntity : class, new()
    {
        internal VenflowDeleteCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand, loggers, shouldLog)
        {
            underlyingCommand.Connection = database.GetConnection();
        }

        ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return default;

            var commandString = new StringBuilder();

            commandString.Append("DELETE FROM ")
                         .AppendLine(EntityConfiguration.TableName)
                         .Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" = ");

            var primaryParameter = EntityConfiguration.PrimaryColumn.ValueRetriever(entity, "0");

            UnderlyingCommand.Parameters.Add(primaryParameter);

            commandString.Append(primaryParameter.ParameterName)
                         .Append(';');

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask<int>(ExecuteBase(Enums.CommandType.DeleteSingle, cancellationToken));
        }

        ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            var commandString = new StringBuilder();

            commandString.Append("DELETE FROM ")
                         .AppendLine(EntityConfiguration.TableName)
                         .Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" IN (");

            var valueRetriever = EntityConfiguration.PrimaryColumn.ValueRetriever;

            var index = 0;

            foreach (var entity in entities)
            {
                var parameter = valueRetriever.Invoke(entity, index++.ToString());

                commandString.Append(parameter.ParameterName)
                             .Append(", ");

                UnderlyingCommand.Parameters.Add(parameter);
            }

            if (index == 0)
                return default;

            commandString.Length -= 2;
            commandString.Append(");");

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask<int>(ExecuteBase(Enums.CommandType.DeleteBatch, cancellationToken));
        }

        ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return default;

            var commandString = new StringBuilder();

            commandString.Append("DELETE FROM ")
                         .AppendLine(EntityConfiguration.TableName)
                         .Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" IN (");

            var valueRetriever = EntityConfiguration.PrimaryColumn.ValueRetriever;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var parameter = valueRetriever.Invoke(entities[i], i.ToString());

                commandString.Append(parameter.ParameterName)
                             .Append(", ");

                UnderlyingCommand.Parameters.Add(parameter);
            }

            commandString.Length -= 2;
            commandString.Append(");");

            UnderlyingCommand.CommandText = commandString.ToString();

            return new ValueTask<int>(ExecuteBase(Enums.CommandType.DeleteBatch, cancellationToken));
        }

        ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return default;

            UnderlyingCommand.CommandText = DeleteBase(entities.AsSpan());

            return new ValueTask<int>(ExecuteBase(Enums.CommandType.DeleteBatch, cancellationToken));
        }

        ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Length == 0)
                return default;

            UnderlyingCommand.CommandText = DeleteBase(entities.AsSpan());

            return new ValueTask<int>(ExecuteBase(Enums.CommandType.DeleteBatch, cancellationToken));
        }

        private string DeleteBase(Span<TEntity> entities)
        {
            var commandString = new StringBuilder();

            commandString.Append("DELETE FROM ")
                         .AppendLine(EntityConfiguration.TableName)
                         .Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" IN (");

            var valueRetriever = EntityConfiguration.PrimaryColumn.ValueRetriever;

            for (int i = entities.Length - 1; i >= 0; i--)
            {
                var parameter = valueRetriever.Invoke(entities[i], i.ToString());

                commandString.Append(parameter.ParameterName)
                             .Append(", ");

                UnderlyingCommand.Parameters.Add(parameter);
            }

            commandString.Length -= 2;
            commandString.Append(");");

            return commandString.ToString();
        }

        private async Task<int> ExecuteBase(Enums.CommandType commandType, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var transaction = await GetTransactionAsync(
#if !NET48
                cancellationToken
#endif
            );

            try
            {
                if (!ShouldAutoCommit)
                    await transaction.SaveAsync(TransactionName, cancellationToken);

                var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                if (ShouldAutoCommit)
                    await transaction.CommitAsync(cancellationToken);

                Log(commandType);

                return affectedRows;
            }
            catch (Exception ex)
            {
                if (ShouldAutoCommit)
                    await transaction.RollbackAsync(cancellationToken);
                else
                    await transaction.RollbackAsync(TransactionName, cancellationToken);

                Log(commandType, ex);

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
