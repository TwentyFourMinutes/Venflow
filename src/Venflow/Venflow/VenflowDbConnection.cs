using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Commands;
using Venflow.Dynamic;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow
{
    public partial class VenflowDbConnection : IAsyncDisposable
    {
        public NpgsqlConnection Connection { get; }

        private readonly DbConfiguration _dbConfiguration;

        internal VenflowDbConnection(DbConfiguration dbConfiguration, NpgsqlConnection connection)
        {
            Connection = connection;
            _dbConfiguration = dbConfiguration;
        }

        #region Misc

        public Task TruncateTableAsync<TEntity>(ForeignTruncateOptions foreignOptions,
            CancellationToken cancellationToken = default) where TEntity : class
            => TruncateTableAsync<TEntity>(IdentityTruncateOptions.None, foreignOptions, cancellationToken);

        public Task TruncateTableAsync<TEntity>(IdentityTruncateOptions truncateOptions = IdentityTruncateOptions.None,
            ForeignTruncateOptions foreignOptions = ForeignTruncateOptions.None,
            CancellationToken cancellationToken = default) where TEntity : class
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

        public Task<int> InsertSingleAsync<TEntity>(TEntity entity, bool returnComputedColumns = false,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = Insert<TEntity>(true).ReturnComputedColumns(returnComputedColumns).Single(entity);

            return InsertSingleAsync(command, returnComputedColumns ? entity : null, cancellationToken);
        }

        public async Task<int> InsertSingleAsync<TEntity>(IInsertCommand<TEntity> insertCommand, TEntity? entity = null,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = (VenflowCommand<TEntity>)insertCommand;

            command.UnderlyingCommand.Connection = Connection;

            if (command.GetComputedColumns && entity is { })
            {
                var value = await command.UnderlyingCommand.ExecuteScalarAsync(cancellationToken);

                command.EntityConfiguration.PrimaryColumn.ValueWriter(entity, value);

                if (command.DisposeCommand)
                    command.Dispose();

                return 1;
            }
            else
            {
                var result = await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

                if (command.DisposeCommand)
                    command.Dispose();

                return result;
            }
        }

        public ValueTask<int> InsertBatchAsync<TEntity>(IEnumerable<TEntity> entities,
            bool returnComputedColumns = false, CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = Insert<TEntity>(true).ReturnComputedColumns(returnComputedColumns).Batch(entities);

            return InsertBatchAsync(command, returnComputedColumns ? entities : null, cancellationToken);
        }

        public async ValueTask<int> InsertBatchAsync<TEntity>(IInsertCommand<TEntity>? insertCommand,
            IEnumerable<TEntity>? entities = null, CancellationToken cancellationToken = default) where TEntity : class
        {
            if (insertCommand is null)
                return 0;

            var command = (VenflowCommand<TEntity>)insertCommand;

            command.UnderlyingCommand.Connection = Connection;

            if (command.GetComputedColumns && entities is { })
            {
                await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

                var valueWriter = command.EntityConfiguration.PrimaryColumn.ValueWriter;

                var index = 0;

                if (entities is IList<TEntity> list)
                {
                    while (await reader.ReadAsync())
                    {
                        valueWriter.Invoke(list[index++], reader.GetValue(0));
                    }
                }
                else
                {
                    foreach (var entity in entities)
                    {
                        if (!await reader.ReadAsync())
                            break;

                        valueWriter.Invoke(entity, reader.GetValue(0));
                    }
                }
            }

            var result = await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (command.DisposeCommand)
                command.Dispose();

            return result;
        }

        #endregion

        #region QueryAsync

        public Task<TEntity?> QuerySingleAsync<TEntity>(bool changeTracking = false,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = Query<TEntity>(true).TrackChanges(changeTracking).Single();

            return QuerySingleAsync(command, cancellationToken);
        }

        public async Task<TEntity?> QuerySingleAsync<TEntity>(IQueryCommand<TEntity> queryCommand,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = (VenflowCommand<TEntity>)queryCommand;

            command.UnderlyingCommand.Connection = Connection;

            await using var reader =
                await command.UnderlyingCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync())
            {
                return default!;
            }

            var isChangeTracking = command.TrackingChanges && command.EntityConfiguration.ChangeTrackerFactory is { };

            var entity = command.EntityConfiguration.QueryCommandCache
                .GetOrCreateFactory(reader.GetColumnSchema(), isChangeTracking).Invoke(reader);

            if (command.DisposeCommand)
                command.Dispose();

            return entity;
        }

        public Task<List<TEntity>> QueryBatchAsync<TEntity>(bool changeTracking = false,
            CancellationToken cancellationToken = default, bool newThing = false) where TEntity : class
        {
            var command = Query<TEntity>(true).TrackChanges(changeTracking).Batch();

            return QueryBatchAsync(command, cancellationToken, newThing);
        }

        public async Task<List<TEntity>> QueryBatchAsync<TEntity>(IQueryCommand<TEntity> queryCommand,
            CancellationToken cancellationToken = default, bool newThing = false) where TEntity : class
        {
            var command = (VenflowCommand<TEntity>)queryCommand;

            command.UnderlyingCommand.Connection = Connection;

            var isChangeTracking = command.TrackingChanges && command.EntityConfiguration.ChangeTrackerFactory is { };

            if (!newThing)
            {
                await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

                var entities = new List<TEntity>();

                var factory = command.EntityConfiguration.QueryCommandCache.GetOrCreateFactory(reader.GetColumnSchema(), isChangeTracking);

                while (await reader.ReadAsync())
                {
                    entities.Add(factory.Invoke(reader));
                }

                if (command.DisposeCommand)
                    command.Dispose();

                return entities;
            }
            else
            {
                await using var reader = await command.UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

                var factory = command.EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer(command.JoinBuilderValues, _dbConfiguration, reader.GetColumnSchema());

                var entities = await factory(reader);

                if (command.DisposeCommand)
                    command.Dispose();

                return entities;
            }
        }

        #endregion

        #region DeleteAsync

        public Task DeleteSingleAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = Delete<TEntity>(true).Single(entity);

            return DeleteAsync(command, cancellationToken);
        }

        public Task DeleteBatchAsync<TEntity>(IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = Delete<TEntity>(true).Batch(entities);

            return DeleteAsync(command, cancellationToken);
        }

        public async Task DeleteAsync<TEntity>(IDeleteCommand<TEntity> deleteCommand,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = (VenflowCommand<TEntity>)deleteCommand;

            command.UnderlyingCommand.Connection = Connection;

            await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (command.DisposeCommand)
                command.Dispose();
        }

        #endregion

        #region UpdateAsync

        public ValueTask<int> UpdateSingleAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var command = Update<TEntity>(true).Single(entity);

            return UpdateAsync(command, cancellationToken);
        }

        public ValueTask<int> UpdateBatchAsync<TEntity>(IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            var command = Update<TEntity>(true).Batch(entities);

            return UpdateAsync(command, cancellationToken);
        }

        public async ValueTask<int> UpdateAsync<TEntity>(IUpdateCommand<TEntity> updateCommand,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            if (updateCommand is null)
                return 0;

            var command = (VenflowCommand<TEntity>)updateCommand;

            command.UnderlyingCommand.Connection = Connection;

            var result = await command.UnderlyingCommand.ExecuteNonQueryAsync(cancellationToken);

            if (command.DisposeCommand)
                command.Dispose();

            return result;
        }

        #endregion

        #region Builder

        public IQueryCommandBuilder<TEntity> Query<TEntity>() where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>()).Query();

        internal IQueryCommandBuilder<TEntity> Query<TEntity>(bool disposeCommand) where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>(), disposeCommand).Query();

        public IInsertCommandBuilder<TEntity> Insert<TEntity>() where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>()).Insert();

        public IInsertCommandBuilder<TEntity> Insert<TEntity>(bool disposeCommand) where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>(), disposeCommand).Insert();

        public IDeleteCommandBuilder<TEntity> Delete<TEntity>() where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>()).Delete();

        public IDeleteCommandBuilder<TEntity> Delete<TEntity>(bool disposeCommand) where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>(), disposeCommand).Delete();

        public IUpdateCommandBuilder<TEntity> Update<TEntity>() where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>()).Update();

        public IUpdateCommandBuilder<TEntity> Update<TEntity>(bool disposeCommand) where TEntity : class
            => new VenflowCommandBuilder<TEntity>(GetEntityConfiguration<TEntity>(), disposeCommand).Update();

        #endregion

        #region Change Tracking

        public void TrackChanges<TEntity>(ref TEntity entity) where TEntity : class
        {
            _dbConfiguration.TrackChanges(ref entity);
        }

        public void TrackChanges<TEntity>(ref IList<TEntity> entities) where TEntity : class
        {
            _dbConfiguration.TrackChanges(ref entities);
        }

        public void TrackChanges<TEntity>(ref IEnumerable<TEntity> entities) where TEntity : class
        {
            _dbConfiguration.TrackChanges(ref entities);
        }

        #endregion

        #region Helpers

        private Entity<TEntity> GetEntityConfiguration<TEntity>() where TEntity : class
        {
            if (!_dbConfiguration.Entities.TryGetValue(typeof(TEntity).Name, out var entityModel))
            {
                throw new TypeArgumentException(
                    "The provided generic type argument doesn't have any configuration class registered in the DbConfiguration.",
                    nameof(TEntity));
            }

            return (Entity<TEntity>)entityModel;
        }

        #endregion

        public ValueTask DisposeAsync()
        {
            return Connection.DisposeAsync();
        }
    }
}