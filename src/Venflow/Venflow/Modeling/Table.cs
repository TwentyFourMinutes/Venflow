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

        public Task TruncateAsync(ForeignTruncateOptions foreignOptions, CancellationToken cancellationToken = default)
            => TruncateAsync(IdentityTruncateOptions.None, foreignOptions, cancellationToken);

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
                case ForeignTruncateOptions.Restric:
                    sb.Append(" RESTRICT");
                    break;
            }

            using var command = new NpgsqlCommand(sb.ToString(), _database.GetConnection());

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            await ValidateConnectionAsync();

            using var command = new NpgsqlCommand("SELECT COUNT(*) FROM " + _configuration.TableName, _database.GetConnection());

            return (long)await command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion

        #region InsertAsync

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Insert(true).Build().InsertAsync(entity, cancellationToken);
        }

        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return insertCommand.InsertAsync(entity, cancellationToken);
        }

        public Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Insert(true).Build().InsertAsync(entities, cancellationToken);
        }

        public Task<int> InsertAsync(IInsertCommand<TEntity> insertCommand, List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)insertCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return insertCommand.InsertAsync(entities, cancellationToken);
        }

        #endregion

        #region QueryAsync

        public Task<TReturn> QueryAsync<TReturn>(IQueryCommand<TEntity, TReturn> queryCommand, CancellationToken cancellationToken = default) where TReturn : class
        {
            ((VenflowBaseCommand<TEntity>)queryCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return queryCommand.QueryAsync(cancellationToken);
        }

        #endregion

        #region DeleteAsync

        public Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entity, cancellationToken);
        }

        public Task<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return deleteCommand.DeleteAsync(entity, cancellationToken);
        }

        public Task<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Delete(true).Build().DeleteAsync(entities, cancellationToken);
        }

        public Task<int> DeleteAsync(IDeleteCommand<TEntity> deleteCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)deleteCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return deleteCommand.DeleteAsync(entities, cancellationToken);
        }

        #endregion

        #region UpdateAsync

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(IUpdateCommand<TEntity> updateCommand, TEntity entity, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return updateCommand.UpdateAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            return Update(true).Build().UpdateAsync(entities, cancellationToken);
        }

        public Task UpdateAsync(IUpdateCommand<TEntity> updateCommand, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            ((VenflowBaseCommand<TEntity>)updateCommand).UnderlyingCommand.Connection = _database.GetConnection();

            return updateCommand.UpdateAsync(entities, cancellationToken);
        }

        #endregion

        #region Builder

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle();

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle(sql);

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).QuerySingle(sql, parameters);

        public IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
           => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QuerySingle(sql, parameters);

        public IPreCommandBuilder<TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryInterpolatedSingle(sql);

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch();

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(count);

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(sql);

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).QueryBatch(sql, parameters);

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, bool disposeCommand, params NpgsqlParameter[] parameters)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryBatch(sql, parameters);

        public IPreCommandBuilder<TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql, bool disposeCommand = true)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).QueryInterpolatedBatch(sql);

        public IInsertCommandBuilder<TEntity> Insert()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Insert();

        public IInsertCommandBuilder<TEntity> Insert(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Insert();

        public IDeleteCommandBuilder<TEntity> Delete()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Delete();

        public IDeleteCommandBuilder<TEntity> Delete(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Delete();

        public IUpdateCommandBuilder<TEntity> Update()
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, false).Update();

        public IUpdateCommandBuilder<TEntity> Update(bool disposeCommand)
            => new VenflowCommandBuilder<TEntity>(_database.GetConnection(), _database, _configuration, disposeCommand).Update();

        #endregion

        #region ChangeTracking

        public void TrackChanges(ref TEntity entity)
        {
            entity = _configuration.ApplyChangeTracking(entity);
        }

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
