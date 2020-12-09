using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Commands;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow
{
    internal interface ITable { }

    /// <summary>
    /// A <see cref="Table{TEntity}"/> is used to perform CRUD operations against the table represented by <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity which represents a table in the Database.</typeparam>
    public sealed class Table<TEntity> : TableBase<TEntity>, ITable
        where TEntity : class, new()
    {
        internal Table(Database database, Entity<TEntity> configuration) : base(database, configuration)
        {
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

            var entityConfiguration = Configuration;

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

            using var command = new NpgsqlCommand(sb.ToString(), Database.GetConnection());

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

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + Configuration.TableName, Database.GetConnection());

            return (long)await command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion

        #region InsertAsync

        /// <summary>
        /// Asynchronously inserts the entity and all entities reachable from the current provided instance into the current table.
        /// </summary>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Insert(true).WithAll().Build().InsertAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts the entity and all entities reachable from the current provided instance into the current table.
        /// </summary>
        /// <param name="insertCommand">A <see cref="IInsertCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return insertCommand.InsertAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously inserts a list of entities and all entities reachable from the current provided instances into the current table.
        /// </summary>
        /// <param name="entities">A list of <typeparamref name="TEntity"/> instance representing the rows, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar'), ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Insert(true).WithAll().Build().InsertAsync(entities, cancellationToken);
        }


        /// <summary>
        /// Asynchronously inserts a list of entities and all entities reachable from the current provided instances into the current table.
        /// </summary>
        /// <param name="insertCommand">A <see cref="IInsertCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A list of <typeparamref name="TEntity"/> instance representing the rows, which will be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows inserted.</returns>
        /// <remarks>This method represents the following SQL statement "INSERT INTO table (foo, bar) VALUES ('foo', 'bar'), ('foo', 'bar')". This API is using parameterized commands.</remarks>
        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return insertCommand.InsertAsync(entities, cancellationToken);
        }

        #endregion

        #region DeleteAsync

        /// <summary>
        /// Asynchronously deletes the provided entity by its defined primary key.
        /// </summary>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entity by its defined primary key.
        /// </summary>
        /// <param name="deleteCommand">A <see cref="IDeleteCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return deleteCommand.DeleteAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes the provided entities by their defined primary keys.
        /// </summary>
        /// <param name="deleteCommand">A <see cref="IDeleteCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows deleted.</returns>
        /// <remarks>This method represents the following SQL statement "DELETE FROM table WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return deleteCommand.DeleteAsync(entities, cancellationToken);
        }

        #endregion

        #region UpdateAsync

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="updateCommand">A <see cref="IUpdateCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance representing the row, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(IUpdateCommand<TEntity> updateCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return updateCommand.UpdateAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Asynchronously updates the provided entity by its defined primary keys.
        /// </summary>
        /// <param name="updateCommand">A <see cref="IUpdateCommand{TEntity}"/> instance which contains all the settings for this operation.</param>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances representing the rows, which will be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        public ValueTask UpdateAsync(IUpdateCommand<TEntity> updateCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = Database.GetConnection();

            return updateCommand.UpdateAsync(entities, cancellationToken);
        }

        #endregion

        #region Builder

        /// <summary>
        /// Creates a new insert command.
        /// </summary>
        /// <returns>A Fluent API Builder for a insert command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IBaseInsertRelationBuilder<TEntity, TEntity> Insert()
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, false).Insert();

        /// <summary>
        /// Creates a new insert command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a insert command.</returns>
        public IBaseInsertRelationBuilder<TEntity, TEntity> Insert(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, disposeCommand).Insert();

        /// <summary>
        /// Creates a new delete command.
        /// </summary>
        /// <returns>A Fluent API Builder for a delete command.</returns>
        /// <remarks>The command will be automatically disposed the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</remarks>
        public IDeleteCommandBuilder<TEntity> Delete()
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, false).Delete();

        /// <summary>
        /// Creates a new delete command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a delete command.</returns>
        public IDeleteCommandBuilder<TEntity> Delete(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, disposeCommand).Delete();

        /// <summary>
        /// Creates a new update command.
        /// </summary>
        /// <returns>A Fluent API Builder for a update command.</returns>
        public IUpdateCommandBuilder<TEntity> Update()
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, false).Update();

        /// <summary>
        /// Creates a new update command.
        /// </summary>
        /// <param name="disposeCommand">Indicates whether or not to dispose the underlying <see cref="NpgsqlCommand"/> after the command got executed once.</param>
        /// <returns>A Fluent API Builder for a update command.</returns>
        public IUpdateCommandBuilder<TEntity> Update(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(Database.GetConnection(), Database, Configuration, disposeCommand).Update();

        #endregion

        #region ChangeTracking

        /// <summary>
        /// Starts tracking the provided <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="entity">A <typeparamref name="TEntity"/> instance which will be change tracked.</param>
        /// <remarks>Any property which should be change tracked on an entity has to be marked virtual.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when the provided entity does not contain any virtual properties.</exception>
        public void TrackChanges(ref TEntity entity)
        {
            entity = Configuration.ApplyChangeTracking(entity);
        }

        /// <summary>
        /// Starts tracking the provided <typeparamref name="TEntity"/>'s.
        /// </summary>
        /// <param name="entities">A set of <typeparamref name="TEntity"/> instances which will be change tracked.</param>
        /// <remarks>This method represents the following SQL statement "UPDATE table SET foo = 'foo' WHERE pk = 0". This API is using parameterized commands.</remarks>
        /// <remarks>Any property which should be change tracked on an entity has to be marked virtual.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when a provided entity does not contain any virtual properties.</exception>
        public void TrackChanges(IList<TEntity> entities)
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                entities[i] = Configuration.ApplyChangeTracking(entities[i]);
            }
        }

        #endregion
    }
}
