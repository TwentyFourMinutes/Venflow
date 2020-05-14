using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Commands;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow
{
    public class VenflowCommandBuilder
    {
        public VenflowDbConnection VenflowDbConnection { get; set; }

        public bool TrackChanges { get; set; }

        internal VenflowCommandBuilder(VenflowDbConnection venflowDbConnection)
        {
            VenflowDbConnection = venflowDbConnection;
        }

        #region QuerySingleAsync

        public Task<TEntity> QuerySingleAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = VenflowDbConnection.CompileQuerySingleCommand<TEntity>();

            venflowCommand.TrackChanges = TrackChanges;

            return VenflowDbConnection.QuerySingleAsync(venflowCommand, cancellationToken);
        }

        public Task<TEntity> QuerySingleAsync<TEntity>(string sql, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = VenflowDbConnection.CompileQueryCommand<TEntity>(sql);

            venflowCommand.TrackChanges = TrackChanges;

            return VenflowDbConnection.QuerySingleAsync(venflowCommand, cancellationToken);
        }

        #endregion

        #region QueryBatchAsync

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = VenflowDbConnection.CompileQueryBatchCommand<TEntity>(-1);

            venflowCommand.TrackChanges = TrackChanges;

            return VenflowDbConnection.QueryBatchAsync(venflowCommand, cancellationToken);
        }

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(int count, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = VenflowDbConnection.CompileQueryBatchCommand<TEntity>(count);

            venflowCommand.TrackChanges = TrackChanges;

            return VenflowDbConnection.QueryBatchAsync(venflowCommand, cancellationToken);
        }

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(string sql, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = VenflowDbConnection.CompileQueryCommand<TEntity>(sql);

            venflowCommand.TrackChanges = TrackChanges;

            return VenflowDbConnection.QueryBatchAsync(venflowCommand, cancellationToken);
        }

        #endregion
    }

    public class VenflowDbConnection : IAsyncDisposable
    {
        public NpgsqlConnection Connection { get; }

        private readonly DbConfiguration _dbConfiguration;

        internal VenflowDbConnection(DbConfiguration dbConfiguration, NpgsqlConnection connection)
        {
            Connection = connection;
            _dbConfiguration = dbConfiguration;
        }

        #region Builder

        public VenflowCommandBuilder TrackChanges()
        {
            return new VenflowCommandBuilder(this) { TrackChanges = true };
        }

        #endregion

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

        //public Task<int> InsertAsync<TEntity>(TEntity entity, bool getInsertedId = true, CancellationToken cancellationToken = default) where TEntity : class
        //{
        //    using var venflowCommand = CompileInsertCommand(entity, getInsertedId);

        //    return InsertAsync(venflowCommand, entity, false, cancellationToken);
        //}

        //public InsertCommand<TEntity> CompileInsertCommand<TEntity>(TEntity entity, bool getInsertedId = true) where TEntity : class
        //    => CompileInsertCommand(entity, true, getInsertedId);

        //public InsertCommand<TEntity> CompileInsertCommand<TEntity>(bool getInsertedId = true) where TEntity : class
        //    => CompileInsertCommand<TEntity>(null, false, getInsertedId);

        //private InsertCommand<TEntity> CompileInsertCommand<TEntity>(TEntity? entity, bool writeParameters, bool getInsertedId) where TEntity : class
        //{
        //    var entityConfiguration = GetEntityConfiguration<TEntity>();

        //    var command = new NpgsqlCommand();

        //    var sb = new StringBuilder();

        //    sb.Append("INSERT INTO ");
        //    sb.Append(entityConfiguration.TableName);
        //    sb.Append(" ");

        //    var startIndex = 1;

        //    if (getInsertedId)
        //    {
        //        getInsertedId = entityConfiguration.PrimaryColumn.IsServerSideGenerated;
        //    }

        //    if (getInsertedId)
        //    {
        //        sb.AppendLine(entityConfiguration.ColumnsString);
        //    }
        //    else
        //    {
        //        sb.AppendLine(entityConfiguration.ColumnsStringWithPrimaryKey);
        //        startIndex = 0;
        //    }

        //    sb.Append("VALUES (");

        //    var i = startIndex;

        //    while (true)
        //    {
        //        if (writeParameters)
        //        {
        //            var parameter = entityConfiguration.Columns[i].ValueRetriever(entity, "0");

        //            command.Parameters.Add(parameter);

        //            sb.Append(parameter.ParameterName);
        //        }
        //        else
        //        {
        //            sb.Append("@" + entityConfiguration.Columns[i].ColumnName + "0");
        //        }

        //        i++;

        //        if (i < entityConfiguration.Columns.Count)
        //        {
        //            sb.Append(", ");
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    sb.Append(")");

        //    if (getInsertedId)
        //    {
        //        sb.Append(" RETURNING \"");

        //        sb.Append(entityConfiguration.PrimaryColumn.ColumnName);

        //        sb.Append('"');
        //    }

        //    sb.Append(";");

        //    command.CommandText = sb.ToString();

        //    return new InsertCommand<TEntity>(command, entityConfiguration)
        //    {
        //        GetInsertedId = getInsertedId,
        //        StartIndex = startIndex
        //    };
        //}

        //public async Task<int> InsertAsync<TEntity>(InsertCommand<TEntity> command, TEntity entity, bool writeParameters, CancellationToken cancellationToken = default) where TEntity : class
        //{
        //    command.UnderlyingCommand.Connection = Connection;

        //    if (writeParameters)
        //    {
        //        command.UnderlyingCommand.Parameters.Clear();

        //        for (int i = command.StartIndex; i < command.EntityConfiguration.Columns.Count; i++)
        //        {
        //            var parameter = command.EntityConfiguration.Columns[i].ValueRetriever(entity, "0");

        //            command.UnderlyingCommand.Parameters.Add(parameter);
        //        }
        //    }

        //    if (command.GetInsertedId)
        //    {
        //        var value = await command.UnderlyingCommand.ExecuteScalarAsync(cancellationToken);

        //        command.EntityConfiguration.PrimaryColumn.ValueWriter(entity, value);

        //        return 1;
        //    }
        //    else
        //    {
        //        return await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
        //    }
        //}

        #endregion

        #region InsertAllAsync

        //public Task<int> InsertAllAsync<TEntity>(IEnumerable<TEntity> entities, bool getInsertedId = false, CancellationToken cancellationToken = default) where TEntity : class
        //{
        //    using var venflowCommand = CompileInsertAllCommand(entities, getInsertedId);

        //    return InsertAllAsync(venflowCommand, entities, false, cancellationToken);
        //}

        //public InsertCommand<TEntity> CompileInsertAllCommand<TEntity>(IEnumerable<TEntity> entities, bool getInsertedId = false) where TEntity : class
        //    => CompileInsertAllCommand(entities, true, getInsertedId);

        //private InsertCommand<TEntity> CompileInsertAllCommand<TEntity>(IEnumerable<TEntity> entities, bool writeParameters, bool getInsertedId = false) where TEntity : class
        //{
        //    var entityConfiguration = GetEntityConfiguration<TEntity>();

        //    var command = new NpgsqlCommand();

        //    var sb = new StringBuilder();

        //    sb.Append("INSERT INTO ");
        //    sb.Append(entityConfiguration.TableName);
        //    sb.Append(" ");

        //    var startIndex = 1;

        //    if (getInsertedId && !entityConfiguration.PrimaryColumn.IsServerSideGenerated)
        //    {
        //        throw new ArgumentException("You can not retrieve the values from the database since this column is not generated in the database.", nameof(getInsertedId));
        //    }

        //    if (getInsertedId || entityConfiguration.PrimaryColumn.IsServerSideGenerated)
        //    {
        //        sb.AppendLine(entityConfiguration.ColumnsString);
        //    }
        //    else
        //    {
        //        sb.AppendLine(entityConfiguration.ColumnsStringWithPrimaryKey);
        //        startIndex = 0;
        //    }

        //    sb.Append("VALUES (");

        //    if (writeParameters)
        //    {
        //        if (entities is IList<TEntity> list)
        //        {
        //            for (int k = 0; k < list.Count; k++)
        //            {
        //                var i = startIndex;

        //                while (true)
        //                {
        //                    var parameter = entityConfiguration.Columns[i].ValueRetriever(list[k], k.ToString());

        //                    command.Parameters.Add(parameter);

        //                    sb.Append(parameter.ParameterName);

        //                    i++;

        //                    if (i < entityConfiguration.Columns.Count)
        //                    {
        //                        sb.Append(", ");
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }

        //                sb.Append("), (");
        //            }
        //        }
        //        else
        //        {
        //            var parameterIndex = 0;

        //            foreach (var entity in entities)
        //            {
        //                var i = startIndex;

        //                while (true)
        //                {
        //                    var parameter = entityConfiguration.Columns[i].ValueRetriever(entity, parameterIndex.ToString());

        //                    command.Parameters.Add(parameter);

        //                    sb.Append(parameter.ParameterName);

        //                    i++;

        //                    if (i < entityConfiguration.Columns.Count)
        //                    {
        //                        sb.Append(", ");
        //                    }
        //                    else
        //                    {
        //                        break;
        //                    }
        //                }

        //                sb.Append("), (");

        //                parameterIndex++;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        int count;

        //        if (entities is IList<TEntity> list)
        //        {
        //            count = list.Count;
        //        }
        //        else
        //        {
        //            count = entities.Count();
        //        }

        //        for (int k = 0; k < count; k++)
        //        {
        //            var i = startIndex;

        //            while (true)
        //            {
        //                sb.Append("@" + entityConfiguration.Columns[i].ColumnName + k.ToString());

        //                i++;

        //                if (i < entityConfiguration.Columns.Count)
        //                {
        //                    sb.Append(", ");
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }

        //            sb.Append("), (");
        //        }
        //    }

        //    sb.Remove(sb.Length - 3, 3);

        //    if (getInsertedId)
        //    {
        //        sb.Append(" RETURNING \"");

        //        sb.Append(entityConfiguration.PrimaryColumn.ColumnName);

        //        sb.Append('"');
        //    }

        //    sb.Append(";");

        //    command.CommandText = sb.ToString();

        //    return new InsertCommand<TEntity>(command, entityConfiguration)
        //    {
        //        GetInsertedId = getInsertedId,
        //        StartIndex = startIndex
        //    };
        //}

        //public async Task<int> InsertAllAsync<TEntity>(InsertCommand<TEntity> command, IEnumerable<TEntity> entities, bool writeParameters, CancellationToken cancellationToken = default) where TEntity : class
        //{
        //    command.UnderlyingCommand.Connection = Connection;

        //    if (writeParameters)
        //    {
        //        var columns = command.EntityConfiguration.Columns;
        //        var parameters = command.UnderlyingCommand.Parameters;

        //        if (entities is IList<TEntity> list)
        //        {
        //            for (int k = 0; k < list.Count; k++)
        //            {
        //                for (int i = command.StartIndex; i < columns.Count; i++)
        //                {
        //                    parameters.Add(columns[i].ValueRetriever(list[k], k.ToString()));
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var parameterIndex = 0;

        //            foreach (var entity in entities)
        //            {
        //                for (int i = command.StartIndex; i < columns.Count; i++)
        //                {
        //                    parameters.Add(columns[i].ValueRetriever(entity, parameterIndex.ToString()));
        //                }

        //                parameterIndex++;
        //            }
        //        }
        //    }

        //    if (command.GetInsertedId)
        //    {
        //        var valueWriter = command.EntityConfiguration.PrimaryColumn.ValueWriter;

        //        await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

        //        foreach (var entity in entities)
        //        {
        //            var success = await reader.ReadAsync(cancellationToken);

        //            if (!success)
        //                break;

        //            valueWriter(entity, reader.GetValue(0));
        //        }

        //        return -1;
        //    }
        //    else
        //    {
        //        return await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
        //    }
        //}

        #endregion

        #region QuerySingleAsync

        public Task<TEntity> QuerySingleAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = CompileQuerySingleCommand<TEntity>();

            return QuerySingleAsync(venflowCommand, cancellationToken);
        }

        public Task<TEntity> QuerySingleAsync<TEntity>(string sql, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            using var venflowCommand = CompileQueryCommand<TEntity>(sql);

            return QuerySingleAsync(venflowCommand, cancellationToken);
        }

        public QueryCommand<TEntity> CompileQuerySingleCommand<TEntity>() where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            return new QueryCommand<TEntity>(new NpgsqlCommand(QueryCommand<TEntity>.CompileDefaultStatement(entityConfiguration)), entityConfiguration);
        }

        public QueryCommand<TEntity> CompileQueryCommand<TEntity>(string sql) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            return new QueryCommand<TEntity>(new NpgsqlCommand(sql), entityConfiguration);
        }

        public async Task<TEntity> QuerySingleAsync<TEntity>(QueryCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class, new()
        {
            command.UnderlyingCommand.Connection = Connection;

            await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync())
            {
                return default!;
            }

            TEntity entity;

            var isChangeTracking = command.TrackChanges && command.EntityConfiguration.ChangeTrackerFactory is { };

            if (isChangeTracking)
            {
                entity = command.EntityConfiguration.GetProxiedEntity();
            }
            else
            {
                entity = new TEntity();
            }

            for (int i = 0; i < reader.VisibleFieldCount; i++)
            {
                var name = reader.GetName(i);

                var value = reader[i];

                command.EntityConfiguration.Columns[name].ValueWriter(entity, value);
            }

            if (isChangeTracking)
            {
                EnableChangeTracking(entity);
            }

            return entity;
        }

        #endregion

        #region QueryBatchAsync

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileQueryBatchCommand<TEntity>(-1);

            return QueryBatchAsync(venflowCommand, cancellationToken);
        }

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(int count, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileQueryBatchCommand<TEntity>(count);

            return QueryBatchAsync(venflowCommand, cancellationToken);
        }

        public Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(string sql, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileQueryCommand<TEntity>(sql);

            return QueryBatchAsync(venflowCommand, cancellationToken);
        }

        public QueryCommand<TEntity> CompileQueryBatchCommand<TEntity>(int count) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            return new QueryCommand<TEntity>(new NpgsqlCommand(QueryCommand<TEntity>.CompileDefaultStatement(entityConfiguration, count)), entityConfiguration);
        }

        public async Task<ICollection<TEntity>> QueryBatchAsync<TEntity>(QueryCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class
        {
            command.UnderlyingCommand.Connection = Connection;

            await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

            return await new ColumnMapper<TEntity>(command).GetEntitiesAsync(reader, cancellationToken);
        }

        #endregion

        #region DeleteOneAsync

        public Task<int> DeleteOneAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileDeleteOneCommand<TEntity>();

            return DeleteOneAsync(venflowCommand, entity, cancellationToken);
        }

        public DeleteCommand<TEntity> CompileDeleteOneCommand<TEntity>() where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();

            var sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.AppendLine(entityConfiguration.TableName);
            sb.Append(" WHERE \"");
            sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
            sb.Append("\" = @");
            sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
            sb.Append("0;");

            return new DeleteCommand<TEntity>(new NpgsqlCommand(sb.ToString()), entityConfiguration);
        }

        public Task<int> DeleteOneAsync<TEntity>(DeleteCommand<TEntity> command, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            command.UnderlyingCommand.Connection = Connection;

            command.UnderlyingCommand.Parameters.Add(command.EntityConfiguration.PrimaryColumn.ValueRetriever(entity, "0"));

            return command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        #region DeleteAllAsync

        public Task<int> DeleteAllAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
        {
            using var venflowCommand = CompileDeleteAllCommandWithParameters(entities);

            return DeleteAllAsync(venflowCommand, cancellationToken);
        }

        private DeleteCommand<TEntity> CompileDeleteAllCommandWithParameters<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();
            var command = new NpgsqlCommand();

            var sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.AppendLine(entityConfiguration.TableName);
            sb.Append(" WHERE \"");
            sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
            sb.Append("\" IN (");

            var valueRetriever = entityConfiguration.PrimaryColumn.ValueRetriever;

            if (entities is IList<TEntity> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append('@');
                    sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
                    sb.Append(i);
                    sb.Append(", ");

                    command.Parameters.Add(valueRetriever(list[i], i.ToString()));
                }
            }
            else
            {
                int i = 0;

                foreach (var entity in entities)
                {
                    sb.Append('@');
                    sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
                    sb.Append(i++);
                    sb.Append(", ");

                    command.Parameters.Add(valueRetriever(entity, i.ToString()));
                }
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(");");

            command.CommandText = sb.ToString();

            return new DeleteCommand<TEntity>(command, entityConfiguration);
        }

        public DeleteCommand<TEntity> CompileDeleteAllCommand<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            var entityConfiguration = GetEntityConfiguration<TEntity>();
            var command = new NpgsqlCommand();

            var sb = new StringBuilder();

            sb.Append("DELETE FROM ");
            sb.AppendLine(entityConfiguration.TableName);
            sb.Append(" WHERE \"");
            sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
            sb.Append("\" IN (");

            if (entities is IList<TEntity> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append('@');
                    sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
                    sb.Append(i);
                    sb.Append(", ");
                }
            }
            else
            {
                int i = 0;

                foreach (var entity in entities)
                {
                    sb.Append('@');
                    sb.Append(entityConfiguration.PrimaryColumn.ColumnName);
                    sb.Append(i++);
                    sb.Append(", ");
                }
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(");");

            command.CommandText = sb.ToString();

            return new DeleteCommand<TEntity>(command, entityConfiguration);
        }

        private Task<int> DeleteAllAsync<TEntity>(DeleteCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class
        {
            command.UnderlyingCommand.Connection = Connection;

            return command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task<int> DeleteAllAsync<TEntity>(DeleteCommand<TEntity> command, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
        {
            var npgsqlCommand = command.UnderlyingCommand;

            npgsqlCommand.Connection = Connection;
            npgsqlCommand.Parameters.Clear();

            var valueRetriever = command.EntityConfiguration.PrimaryColumn.ValueRetriever;

            if (entities is IList<TEntity> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    npgsqlCommand.Parameters.Add(valueRetriever(list[i], i.ToString()));
                }
            }
            else
            {
                int i = 0;

                foreach (var entity in entities)
                {
                    npgsqlCommand.Parameters.Add(valueRetriever(entity, i.ToString()));
                }
            }

            return command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        #endregion

        public Task GetPreparedCommandAsync<TEntity>(VenflowCommand<TEntity> command, CancellationToken cancellationToken = default) where TEntity : class
        {
            if (command.UnderlyingCommand.Connection is null || command.UnderlyingCommand.Connection.State != ConnectionState.Open)
            {
                command.UnderlyingCommand.Connection = Connection;
            }

            return command.PerpareSelfAsync(cancellationToken);
        }

        private Entity<TEntity> GetEntityConfiguration<TEntity>() where TEntity : class
        {
            if (!_dbConfiguration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException("The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.", nameof(TEntity));
            }

            return (Entity<TEntity>)entityModel;
        }

        private void EnableChangeTracking<TEntity>(TEntity entity) where TEntity : class
        {
            ((IEntityProxy<TEntity>)entity).ChangeTracker.TrackChanges = true;
        }

        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}