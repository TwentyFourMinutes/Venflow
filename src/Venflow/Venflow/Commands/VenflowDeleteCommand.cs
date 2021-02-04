using System;
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
        internal VenflowDeleteCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            underlyingCommand.Connection = database.GetConnection();
        }

        async ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return 0;

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

            await ValidateConnectionAsync();

            var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                this.DisposeAsync();

            return affectedRows;
        }

        async ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
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
                return 0;

            commandString.Length -= 2;
            commandString.Append(");");

            UnderlyingCommand.CommandText = commandString.ToString();

            await ValidateConnectionAsync();

            try
            {
                return await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

        async ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return 0;

            await ValidateConnectionAsync();

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

            var transaction = await Database.BeginTransactionAsync(
#if NET5_0
                cancellationToken
#endif
            );

            try
            {
                var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            finally
            {
                await transaction.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

        async ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return 0;

            await ValidateConnectionAsync();

            UnderlyingCommand.CommandText = DeleteBase(entities.AsSpan());

            var transaction = await Database.BeginTransactionAsync(
#if NET5_0
                cancellationToken
#endif
            );

            try
            {
                var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            finally
            {
                await transaction.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
        }

        async ValueTask<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Length == 0)
                return 0;

            await ValidateConnectionAsync();

            UnderlyingCommand.CommandText = DeleteBase(entities.AsSpan());

            var transaction = await Database.BeginTransactionAsync(
#if NET5_0
                cancellationToken
#endif
            );

            try
            {
                var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return affectedRows;
            }
            finally
            {
                await transaction.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
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

        public ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            return new ValueTask();
        }
    }
}
