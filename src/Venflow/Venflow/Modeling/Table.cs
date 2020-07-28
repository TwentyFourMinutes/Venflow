using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Commands;
using Venflow.Enums;

namespace Venflow.Modeling
{
    /// <summary>
    /// A <see cref="Table{TEntity}"/> is used to perform CRUD operations against the table represented by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity which represents a table in the Database.</typeparam>
    public sealed class Table<TEntity> where TEntity : class
    {
        private readonly Database _database;
        private readonly Entity<TEntity> _configuration;

        internal Table(Database database, Entity<TEntity> configuration)
        {
            _database = database;
            _configuration = configuration;
        }

        #region Misc

        /// <summary>
        /// Asynchronously truncates the current table with the provided options.
        /// </summary>
        /// <param name="foreignOptions">Specifies how the truncate operation should handle foreign tables.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "TRUNCATE table [CASCADE|RESTRICT|NONE]".</remarks>
        public Task TruncateAsync(ForeignTruncateOptions foreignOptions, CancellationToken cancellationToken = default)
            => TruncateAsync(IdentityTruncateOptions.None, foreignOptions, cancellationToken);

        /// <summary>
        /// Asynchronously truncates the current table with the provided options.
        /// </summary>
        /// <param name="truncateOptions">Specifies how the truncate operation should handle identities in the table.</param>
        /// <param name="foreignOptions">Specifies how the truncate operation should handle foreign tables.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "TRUNCATE table [RESTART IDENTITY|CONTINUE IDENTITY|NONE] [CASCADE|RESTRICT|NONE]".</remarks>
        public async Task TruncateAsync(IdentityTruncateOptions truncateOptions = IdentityTruncateOptions.None, ForeignTruncateOptions foreignOptions = ForeignTruncateOptions.None, CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            var entityConfiguration = _configuration;

            var sb = new StringBuilder()
                        .Append("TRUNCATE ")
                        .Append(entityConfiguration.TableName);

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
                case ForeignTruncateOptions.Restrict:
                    sb.Append(" RESTRICT");
                    break;
            }

            using var command = new NpgsqlCommand(sb.ToString(), _database.GetConnection());

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously counts the total rows the current table.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows.</returns>
        /// <remarks>This method represents the following SQL statement "SELECT COUNT(*) FROM table".</remarks>
        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + _configuration.TableName, _database.GetConnection());

            return (long)await command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion

        #region InsertAsync

        /// <summary>
        /// Asynchronously inserts the entity and all entities reachable from the current provided instance into the current table.
        /// </summary>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Insert(true).Build().InsertAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts the entity and all entities reachable from the current provided instance into the current table.
        /// </summary>
        /// <param name="insertCommand">A <see cref="IInsertCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return insertCommand.InsertAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts a list of entities and all entities reachable from the current provided instances into the current table.
        /// </summary>
        /// <param name="entities">A list of <see cref="TEntity"/> instance representing the rows, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar'), ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Insert(true).Build().InsertAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts a list of entities and all entities reachable from the current provided instances into the current table.
        /// </summary>
        /// <param name="insertCommand">A <see cref="IInsertCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A list of <see cref="TEntity"/> instance representing the rows, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar'), ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return insertCommand.InsertAsync(entities, cancellationToken);
        }

        #endregion

        #region QueryAsync

        /// <summary>
        /// Asynchronously queries one or more entities with the configured joins.
        /// </summary>
        /// <param name="queryCommand">A <see cref="IQueryCommand{TEntity, TReturn}"/> instance representing the query which will be performed.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the result of the executed query.</returns>
        /// <remarks>This method could represents the following SQL statement "SELECT * FROM table".</remarks>
        public Task<TReturn> QueryAsync<TReturn>(IQueryCommand<TEntity, TReturn> queryCommand, CancellationToken cancellationToken = default) where TReturn : class
        {
            ((VenflowBaseCommand<TEntity>)queryCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return queryCommand.QueryAsync(cancellationToken);
        }

        #endregion

        #region DeleteAsync

        /// <summary>
        /// Asynchronously deletes the provided entity by its defined primary key.
        /// </summary>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entity by its defined primary key.
        /// </summary>
        /// <param name="deleteCommand">A <see cref="IDeleteCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return deleteCommand.DeleteAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <see cref="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="deleteCommand">A <see cref="IDeleteCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A set of <see cref="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return deleteCommand.DeleteAsync(entities, cancellationToken);
        }

        #endregion

        #region UpdateAsync

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="updateCommand">A <see cref="IUpdateCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <see cref="TEntity"/> instance representing the row, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task UpdateAsync(IUpdateCommand<TEntity> updateCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return updateCommand.UpdateAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <see cref="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="updateCommand">A <see cref="IUpdateCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A set of <see cref="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public Task UpdateAsync(IUpdateCommand<TEntity> updateCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return updateCommand.UpdateAsync(entities, cancellationToken);
        }

        #endregion

        #region Builder

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>This method will automatically generate a SQL SELECT statement.</remarks>
        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle();

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle(sql);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, true).QuerySingle(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
           => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a single returned primary row. <strong>This API does< support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. You should only add parameters trough string interpolation.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryInterpolatedSingle(sql);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>This method will automatically generate a SQL SELECT statement.</remarks>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch();

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="count">A value specifying the maximum returned primary rows.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>This method will automatically generate a SQL SELECT statement with a LIMIT.</remarks>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(count);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you need to pass parameters with the query, either use <see cref="QuerySingle(string, NpgsqlParameter[])"/> or <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(sql);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned.<strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).QueryBatch(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does not support string interpolation!</strong> If you want to pass interpolated SQL use <see cref="QueryInterpolatedSingle(FormattableString, bool)"/>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter.</param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <param name="parameters">A set of <see cref="NpgsqlParameter"/> which contain parameters for the <paramref name="sql"/> command.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(sql, parameters);

        /// <summary>
        /// Creates a new query command, which expects a set of primary rows to be returned. <strong>This API does support string interpolation!</strong>.
        /// </summary>
        /// <param name="sql">A string containing the SQL statement. Ensure that you do not pass any user manipulated SQL for this parameter. <strong>You should only add parameters trough string interpolation.</strong></param>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a query command.</returns>
        public IPreCommandBuilder<TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryInterpolatedBatch(sql);

        /// <summary>
        /// Creates a new insert command.
        /// </summary>
        /// <returns>A Fluent API Builder for a insert command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IInsertCommandBuilder<TEntity> Insert()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Insert();

        /// <summary>
        /// Creates a new insert command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a insert command.</returns>
        public IInsertCommandBuilder<TEntity> Insert(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Insert();

        /// <summary>
        /// Creates a new delete command.
        /// </summary>
        /// <returns>A Fluent API Builder for a delete command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IDeleteCommandBuilder<TEntity> Delete()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Delete();

        /// <summary>
        /// Creates a new delete command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a delete command.</returns>
        public IDeleteCommandBuilder<TEntity> Delete(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Delete();

        /// <summary>
        /// Creates a new update command.
        /// </summary>
        /// <returns>A Fluent API Builder for a update command.</returns>
        public IUpdateCommandBuilder<TEntity> Update()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Update();

        /// <summary>
        /// Creates a new update command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a update command.</returns>
        public IUpdateCommandBuilder<TEntity> Update(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Update();

        #endregion

        #region ChangeTracking

        /// <summary>
        /// Starts tracking the provided <see cref="TEntity"/>.
        /// </summary>
        /// <param name="entity">A <see cref="TEntity"/> instance which will be change tracked.</param>
        /// <remarks>Any property which should be change tracked on an entity has to be marked virtual.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when the provided entity does not contain any virtual properties.</exception>
        public void TrackChanges(ref TEntity entity)
        {
            entity = _configuration.ApplyChangeTracking(entity);
        }

        /// <summary>
        /// Starts tracking the provided <see cref="TEntity"/>'s.
        /// </summary>
        /// <param name="entities">A set of <see cref="TEntity"/> instances which will be change tracked.</param>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        /// <remarks>Any property which should be change tracked on an entity has to be marked virtual.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when a provided entity does not contain any virtual properties.</exception>
        public void TrackChanges(IList<TEntity> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i] = _configuration.ApplyChangeTracking(entities[i]);
            }
        }

        #endregion

        private ValueTask ValidateConnectionAsync()
        {
            var connection = _database.GetConnection();

            if (connection.State == ConnectionState.Open)
                return default;

            if (connection.State == ConnectionState.Closed)
            {
                return new ValueTask(connection.OpenAsync());
            }
            else
            {
                throw new InvalidOperationException($"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{connection.State}'.");
            }
        }
    }
}
