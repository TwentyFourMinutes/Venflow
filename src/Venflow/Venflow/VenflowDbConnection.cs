using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    public class VenflowDbConnection : IAsyncDisposable
    {
        public NpgsqlConnection Connection { get; }

        private readonly DbConfiguration _dbConfiguration;

        internal VenflowDbConnection(DbConfiguration dbConfiguration, NpgsqlConnection connection)
        {
            Connection = connection;
            _dbConfiguration = dbConfiguration;
        }

        public async Task<int> InsertAsync<TEntity>(TEntity entity, bool getInstertedId = true, CancellationToken cancellationToken = default) where TEntity : class
        {
            if (!_dbConfiguration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            using var command = new NpgsqlCommand
            {
                Connection = Connection
            };

            var sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(entityConfiguration.TableName);
            sb.Append(" ");

            var startIndex = 1;

            if (getInstertedId)
            {
                getInstertedId = entityConfiguration.PrimaryColumn.IsServerSideGenerated;
            }

            if (getInstertedId)
            {
                sb.AppendLine(entityConfiguration.ColumnsString);
            }
            else
            {
                sb.AppendLine(entityConfiguration.ColumnsStringWithPrimaryKey);
                startIndex = 0;
            }

            sb.Append("VALUES (");

            var i = startIndex;

            while (true)
            {
                var parameter = entityConfiguration.Columns[i].ParameterRetriever(entity, "0");

                command.Parameters.Add(parameter);

                sb.Append(parameter.ParameterName);

                i++;

                if (i < entityConfiguration.Columns.Length)
                {
                    sb.Append(", ");
                }
                else
                {
                    break;
                }
            }

            sb.Remove(sb.Length - 2, 2);

            sb.Append(")");

            if (getInstertedId)
            {
                sb.Append(" RETURNING \"");

                sb.Append(entityConfiguration.PrimaryColumn.ColumnName);

                sb.Append('"');
            }

            sb.Append(";");

            command.CommandText = sb.ToString();

            if (getInstertedId)
            {
                var value = await command.ExecuteScalarAsync(cancellationToken);

                entityConfiguration.PrimaryColumn.ValueWriter(entity, value);

                return -1;
            }
            else
            {
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task<int> InsertAllAsync<TEntity>(IEnumerable<TEntity> entities, bool getInstertedId = false, CancellationToken cancellationToken = default) where TEntity : class
        {
            if (!_dbConfiguration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
            }

            var entityConfiguration = (Entity<TEntity>)entityModel;

            using var command = new NpgsqlCommand
            {
                Connection = Connection
            };

            var sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(entityConfiguration.TableName);
            sb.Append(" ");

            var startIndex = 1;

            if (getInstertedId)
            {
                getInstertedId = entityConfiguration.PrimaryColumn.IsServerSideGenerated;
            }

            if (getInstertedId)
            {
                sb.AppendLine(entityConfiguration.ColumnsString);
            }
            else
            {
                sb.AppendLine(entityConfiguration.ColumnsStringWithPrimaryKey);
                startIndex = 0;
            }

            sb.Append("VALUES (");

            var parameterIndex = 0;

            foreach (var entity in entities)
            {
                var i = startIndex;

                while(true)
                {
                    var parameter = entityConfiguration.Columns[i].ParameterRetriever(entity, parameterIndex.ToString());

                    command.Parameters.Add(parameter);

                    sb.Append(parameter.ParameterName);

                    i++;

                    if (i < entityConfiguration.Columns.Length)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        break;
                    }
                }

                sb.Append("), (");

                parameterIndex++;
            }

            sb.Remove(sb.Length - 3, 3);

            if (getInstertedId)
            {
                sb.Append(" RETURNING \"");

                sb.Append(entityConfiguration.PrimaryColumn.ColumnName);

                sb.Append('"');
            }

            sb.Append(";");

            command.CommandText = sb.ToString();

            if (getInstertedId)
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                foreach (var entity in entities)
                {
                    var success = await reader.ReadAsync(cancellationToken);

                    if (!success)
                        break;

                    var value = reader.GetValue(0);

                    entityConfiguration.PrimaryColumn.ValueWriter(entity, value);
                }

                return -1;
            }
            else
            {
                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}
