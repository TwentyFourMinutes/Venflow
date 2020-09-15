using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic;
using Venflow.Dynamic.Proxies;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommand<TEntity> : VenflowBaseCommand<TEntity>, IUpdateCommand<TEntity> where TEntity : class, new()
    {
        internal VenflowUpdateCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
        }


        async ValueTask IUpdateCommand<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            if (entity is null)
                return;

            var commandString = new StringBuilder();

            BaseUpdate(entity, 0, commandString);

            if (commandString.Length == 0)
            {
                return;
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            await ValidateConnectionAsync();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        async ValueTask IUpdateCommand<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null)
                return;

            var commandString = new StringBuilder();

            var index = 0;

            foreach (var entity in entities)
            {
                BaseUpdate(entity, index++, commandString);
            }
            if (index == 0 ||
                commandString.Length == 0)
            {
                return;
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            await ValidateConnectionAsync();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        async ValueTask IUpdateCommand<TEntity>.UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return;

            var commandString = BaseUpdate(entities.AsSpan());

            if (commandString.Length == 0)
            {
                return;
            }

            UnderlyingCommand.CommandText = commandString;

            await ValidateConnectionAsync();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        async ValueTask IUpdateCommand<TEntity>.UpdateAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Length == 0)
                return;

            var commandString = BaseUpdate(entities);

            if (commandString.Length == 0)
            {
                return;
            }

            UnderlyingCommand.CommandText = commandString;

            await ValidateConnectionAsync();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        async ValueTask IUpdateCommand<TEntity>.UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            if (entities is null ||
                entities.Count == 0)
                return;

            var commandString = new StringBuilder();


            for (int i = 0; i < entities.Count; i++)
            {
                BaseUpdate(entities[i], i, commandString);
            }

            if (commandString.Length == 0)
            {
                return;
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            await ValidateConnectionAsync();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        private string BaseUpdate(Span<TEntity> entities)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < entities.Length; i++)
            {
                BaseUpdate(entities[i], i++, stringBuilder);
            }

            return stringBuilder.ToString();
        }

        private void BaseUpdate(TEntity entity, int index, StringBuilder commandString)
        {
            if (!(entity is IEntityProxy<TEntity> proxy))
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

                var columns = proxy.ChangeTracker.GetColumns()!;

                var entityColumns = EntityConfiguration.Columns.Values.AsSpan();

                for (int i = 0; i < columns.Length; i++)
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

            return new ValueTask();
        }
    }
}
