using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Dynamic.Proxies;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommand<TEntity> : VenflowBaseCommand<TEntity>, IUpdateCommand<TEntity> where TEntity : class
    {
        internal VenflowUpdateCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {

        }


        async Task IUpdateCommand<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var commandString = new StringBuilder();

            BaseUpdate(entity, 0, commandString);

            UnderlyingCommand.CommandText = commandString.ToString();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
        }

        async Task IUpdateCommand<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var commandString = new StringBuilder();

            if (entities is IList<TEntity> entitiesList)
            {
                for (int i = 0; i < entitiesList.Count; i++)
                {
                    BaseUpdate(entitiesList[i], i, commandString);
                }
            }
            else
            {
                var index = 0;

                foreach (var entity in entities)
                {
                    BaseUpdate(entity, index++, commandString);
                }
            }

            UnderlyingCommand.CommandText = commandString.ToString();

            await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                await this.DisposeAsync();
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

                var entityColumns = EntityConfiguration.Columns;

                for (int i = 0; i < columns.Length; i++)
                {
                    var columnIndex = columns[i];

                    if (columnIndex == 0)
                        continue;

                    columns[i] = 0;

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

            var primaryParameter = EntityConfiguration.PrimaryColumn.ValueRetriever(entity, "PK");

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
