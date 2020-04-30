using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Enums;

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

        #region Misc

        public Task TruncateTableAsync<TEntity>(ForeignTruncateOptions foreignOptions, CancellationToken cancellationToken = default) where TEntity : class
            => TruncateTableAsync<TEntity>(IdentityTruncateOptions.None, foreignOptions, cancellationToken);

        public Task TruncateTableAsync<TEntity>(IdentityTruncateOptions truncateOptions = IdentityTruncateOptions.None, ForeignTruncateOptions foreignOptions = ForeignTruncateOptions.None, CancellationToken cancellationToken = default) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            var sb = new StringBuilder();
            sb.Append("TRUNCATE ");
            sb.Append(entityConfiguration.TableName);

            switch (truncateOptions)
            {
                case IdentityTruncateOptions.None:
                    break;
                case IdentityTruncateOptions.Restart:
                    sb.Append(" RESTART IDENTITY");
                    break;
                case IdentityTruncateOptions.Continue:
                    sb.Append(" CONTINUE IDENTITY");
                    break;
            }

            switch (foreignOptions)
            {
                case ForeignTruncateOptions.None:
                    break;
                case ForeignTruncateOptions.Cascade:
                    sb.Append(" CASCADE");
                    break;
                case ForeignTruncateOptions.Restric:
                    sb.Append(" RESTRICT");
                    break;
            }

            using var command = new NpgsqlCommand(sb.ToString(), Connection);

            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task<long> CountAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + entityConfiguration.TableName, Connection);

            return command.ExecuteScalarAsync(cancellationToken).ContinueWith(task => (long)task.Result);
        }

        #endregion

        #region InsertAsync

        public Task<int> InsertAsync<TEntity>(TEntity entity, bool getInstertedId = true, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileInsertCommand(entity, getInstertedId);

            return InsertAsync(venflowCommand, entity, cancellationToken);
        }

        public VenflowCommand<TEntity> CompileInsertCommand<TEntity>(TEntity entity, bool getInstertedId = true) where TEntity : class
            => CompileInsertCommand(entity, true, getInstertedId);

        public VenflowCommand<TEntity> CompileInsertCommand<TEntity>(bool getInstertedId = true) where TEntity : class
            => CompileInsertCommand<TEntity>(null, false, getInstertedId);

        private VenflowCommand<TEntity> CompileInsertCommand<TEntity>(TEntity? entity, bool writeParameters, bool getInstertedId) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            var command = new NpgsqlCommand();

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
                if (writeParameters)
                {
                    var parameter = entityConfiguration.Columns[i].ValueRetriever(entity, "0");

                    command.Parameters.Add(parameter);

                    sb.Append(parameter.ParameterName);
                }
                else
                {
                    sb.Append("@" + entityConfiguration.Columns[i].ColumnName + "0");
                }

                i++;

                if (i < entityConfiguration.Columns.Count)
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

            return new VenflowCommand<TEntity>(command, entityConfiguration)
            {
                GetInstertedId = getInstertedId
            };
        }

        public async Task<int> InsertAsync<TEntity>(VenflowCommand<TEntity> command, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            command.UnderlyingCommand.Connection = Connection;

            if (command.GetInstertedId)
            {
                var value = await command.UnderlyingCommand.ExecuteScalarAsync(cancellationToken);

                command.EntityConfiguration.PrimaryColumn.ValueWriter(entity, value);

                return -1;
            }
            else
            {
                return await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        #endregion

        #region InsertAllAsync

        public Task<int> InsertAllAsync<TEntity>(IEnumerable<TEntity> entities, bool getInstertedId = false, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileInsertAllCommand(entities, getInstertedId);

            return InsertAllAsync(venflowCommand, entities, cancellationToken);
        }

        public VenflowCommand<TEntity> CompileInsertAllCommand<TEntity>(IEnumerable<TEntity> entities, bool getInstertedId = false) where TEntity : class
            => CompileInsertAllCommand(entities, 0, true, getInstertedId);

        public VenflowCommand<TEntity> CompileInsertAllCommand<TEntity>(int itemCount, bool getInstertedId = false) where TEntity : class
            => CompileInsertAllCommand<TEntity>(null, itemCount, true, getInstertedId);

        private VenflowCommand<TEntity> CompileInsertAllCommand<TEntity>(IEnumerable<TEntity>? entities, int itemCount, bool writeParameters, bool getInstertedId = false) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            var command = new NpgsqlCommand();

            var sb = new StringBuilder();

            sb.Append("INSERT INTO ");
            sb.Append(entityConfiguration.TableName);
            sb.Append(" ");

            var startIndex = 1;

            if (getInstertedId)
            {
                getInstertedId = entityConfiguration.PrimaryColumn.IsServerSideGenerated;
            }

            if (getInstertedId || entityConfiguration.PrimaryColumn.IsServerSideGenerated)
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

            if (writeParameters)
            {
                foreach (var entity in entities)
                {
                    var i = startIndex;

                    while (true)
                    {
                        var parameter = entityConfiguration.Columns[i].ValueRetriever(entity, parameterIndex.ToString());

                        command.Parameters.Add(parameter);

                        sb.Append(parameter.ParameterName);

                        i++;

                        if (i < entityConfiguration.Columns.Count)
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
            }
            else
            {
                for (int k = 0; k < itemCount; k++)
                {
                    var i = startIndex;

                    while (true)
                    {
                        sb.Append("@" + entityConfiguration.Columns[i].ColumnName + parameterIndex.ToString());

                        i++;

                        if (i < entityConfiguration.Columns.Count)
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

            return new VenflowCommand<TEntity>(command, entityConfiguration)
            {
                GetInstertedId = getInstertedId
            };
        }

        public async Task<int> InsertAllAsync<TEntity>(VenflowCommand<TEntity> command, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
        {
            command.UnderlyingCommand.Connection = Connection;

            if (command.GetInstertedId)
            {
                var valueWriter = command.EntityConfiguration.PrimaryColumn.ValueWriter;

                await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

                foreach (var entity in entities)
                {
                    var success = await reader.ReadAsync(cancellationToken);

                    if (!success)
                        break;

                    valueWriter(entity, reader.GetValue(0));
                }

                return -1;
            }
            else
            {
                return await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        #endregion

        #region QuerySelectAsync

        public Task<TEntity> QuerySingleAsync<TEntity>(string sql, bool orderPreservedColumns = false, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = CompileQuerySingleCommand<TEntity>(sql, orderPreservedColumns);

            return QuerySingleAsync(venflowCommand, cancellationToken);
        }

        public VenflowCommand<TEntity> CompileQuerySingleCommand<TEntity>(string sql, bool orderPreservedColumns = false) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            return new VenflowCommand<TEntity>(new NpgsqlCommand(sql), entityConfiguration)
            {
                OrderPreservedColumns = orderPreservedColumns
            };
        }

        public async Task<TEntity> QuerySingleAsync<TEntity>(VenflowCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            command.UnderlyingCommand.Connection = Connection;

            await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync())
            {
                return default!;
            }

            var entity = new TEntity();

            if (command.OrderPreservedColumns)
            {
                int counter = reader.VisibleFieldCount - 1;

                for (int i = 0; i < reader.VisibleFieldCount; i++)
                {
                    var value = reader[counter--];

                    command.EntityConfiguration.Columns[i].ValueWriter(entity, value);
                }
            }
            else
            {
                for (int i = 0; i < reader.VisibleFieldCount; i++)
                {
                    var name = reader.GetName(i);

                    var value = reader[i];

                    command.EntityConfiguration.Columns[name].ValueWriter(entity, value);
                }
            }

            return entity;
        }

        #endregion

        #region QueryAllAsync

        public Task<ICollection<TEntity>> QueryAllAsync<TEntity>(string sql, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = CompileQueryAllCommand<TEntity>(sql);

            return QueryAllAsync(venflowCommand, cancellationToken);
        }

        public VenflowCommand<TEntity> CompileQueryAllCommand<TEntity>(string sql) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            return new VenflowCommand<TEntity>(new NpgsqlCommand(sql), entityConfiguration);
        }

        public async Task<ICollection<TEntity>> QueryAllAsync<TEntity>(VenflowCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            command.UnderlyingCommand.Connection = Connection;

            await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

            return await new ColumnMapper<TEntity>(command).GetEntitiesAsync(reader, cancellationToken);
        }

        #endregion

        public Task<PreparedCommand<TEntity>> GetPreparedCommandAsync<TEntity>(VenflowCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class
        {
            return PreparedCommand<TEntity>.CreateAsync(this.Connection, command, cancellationToken);
        }

        private Entity<TEntity> GetEntityConfiguration<TEntity>() where TEntity : class
        {
            if (!_dbConfiguration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
            }

            return (Entity<TEntity>)entityModel;
        }

        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}