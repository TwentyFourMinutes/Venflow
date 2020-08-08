using Npgsql;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommand<TEntity> : VenflowBaseCommand<TEntity>, IDeleteCommand<TEntity> where TEntity : class, new()
    {
        internal VenflowDeleteCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, bool disposeCommand) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {

        }

        async Task<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

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

            var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (DisposeCommand)
                this.DisposeAsync();

            return affectedRows;
        }

        async Task<int> IDeleteCommand<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var commandString = new StringBuilder();

            commandString.Append("DELETE FROM ")
                         .AppendLine(EntityConfiguration.TableName)
                         .Append(" WHERE \"")
                         .Append(EntityConfiguration.PrimaryColumn.ColumnName)
                         .Append("\" IN (");

            var valueRetriever = EntityConfiguration.PrimaryColumn.ValueRetriever;

            if (entities is IList<TEntity> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var parameter = valueRetriever.Invoke(list[i], i.ToString());

                    commandString.Append(parameter.ParameterName)
                                 .Append(", ");

                    UnderlyingCommand.Parameters.Add(parameter);
                }
            }
            else
            {
                var index = 0;

                foreach (var entity in entities)
                {
                    var parameter = valueRetriever.Invoke(entity, index++.ToString());

                    commandString.Append(parameter.ParameterName)
                                 .Append(", ");

                    UnderlyingCommand.Parameters.Add(parameter);
                }
            }

            commandString.Length -= 2;
            commandString.Append(");");

            UnderlyingCommand.CommandText = commandString.ToString();

            var affectedRows = await UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

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
