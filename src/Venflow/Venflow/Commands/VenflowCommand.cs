using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommand<TEntity> : IQueryCommand<TEntity>, IInsertCommand<TEntity>, IDeleteCommand<TEntity>, IUpdateCommand<TEntity> where TEntity : class
    {
        internal JoinBuilderValues? JoinBuilderValues { get; set; }

        internal bool IsSingle { get; set; }
        internal bool TrackingChanges { get; set; }
        internal bool GetComputedColumns { get; set; }
        internal bool DisposeCommand { get; set; }

        internal DbConfiguration DbConfiguration { get; }
        internal Entity<TEntity> EntityConfiguration { get; }
        internal NpgsqlCommand UnderlyingCommand { get; }

        internal Delegate? Delegate { get; private set; }

        internal VenflowCommand(DbConfiguration dbConfiguration, Entity<TEntity> entity, NpgsqlCommand underlyingCommand)
        {
            DbConfiguration = dbConfiguration;
            EntityConfiguration = entity;
            UnderlyingCommand = underlyingCommand;
        }

        Task<IQueryCommand<TEntity>> IQueryCommand<TEntity>.PrepareAsync(CancellationToken cancellationToken)
        {
            return UnderlyingCommand.PrepareAsync(cancellationToken).ContinueWith(_ => (IQueryCommand<TEntity>)this);
        }

        IQueryCommand<TEntity> IQueryCommand<TEntity>.Unprepare()
        {
            UnderlyingCommand.Unprepare();

            return this;
        }

        async Task<TEntity?> IQueryCommand<TEntity>.QuerySingleAsync(CancellationToken cancellationToken)
        {
            EnsureValidConnection();

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync())
            {
                return default!;
            }

            Func<NpgsqlDataReader, Task<List<TEntity>>> materializer;

            if (Delegate is { })
            {
                materializer = (Func<NpgsqlDataReader, Task<List<TEntity>>>)Delegate;
            }
            else
            {
                Delegate = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer(JoinBuilderValues, DbConfiguration, reader.GetColumnSchema(), TrackingChanges && EntityConfiguration.ChangeTrackerFactory is { });
            }

            var entity = (await materializer(reader)).FirstOrDefault(); // TODO: Refactor code to build more efficient materializer

            if (DisposeCommand)
                this.Dispose();

            return entity;
        }

        async Task<List<TEntity>> IQueryCommand<TEntity>.QueryBatchAsync(CancellationToken cancellationToken)
        {
            EnsureValidConnection();

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

            Func<NpgsqlDataReader, Task<List<TEntity>>> materializer;

            if (Delegate is { })
            {
                materializer = (Func<NpgsqlDataReader, Task<List<TEntity>>>)Delegate;
            }
            else
            {
                Delegate = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer(JoinBuilderValues, DbConfiguration, reader.GetColumnSchema(), TrackingChanges);
            }

            var entities = await materializer(reader);

            if (DisposeCommand)
                this.Dispose();

            return entities;
        }

        Task<IInsertCommand<TEntity>> IInsertCommand<TEntity>.PrepareAsync(CancellationToken cancellationToken)
        {
            return UnderlyingCommand.PrepareAsync(cancellationToken).ContinueWith(_ => (IInsertCommand<TEntity>)this);
        }

        IInsertCommand<TEntity> IInsertCommand<TEntity>.Unprepare()
        {
            UnderlyingCommand.Unprepare();

            return this;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            EnsureValidConnection();

            Func<NpgsqlConnection, List<TEntity>, Task<int>> inserter;

            if (Delegate is { })
            {
                inserter = (Func<NpgsqlConnection, List<TEntity>, Task<int>>)Delegate;
            }
            else
            {
                Delegate = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter(DbConfiguration);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, entities);

            if (DisposeCommand)
                this.Dispose();

            return affectedRows;
        }

        async Task<int> IInsertCommand<TEntity>.InsertAsync(TEntity entity, CancellationToken cancellationToken)
        {
            EnsureValidConnection();

            Func<NpgsqlConnection, List<TEntity>, Task<int>> inserter;

            if (Delegate is { })
            {
                inserter = (Func<NpgsqlConnection, List<TEntity>, Task<int>>)Delegate;
            }
            else
            {
                Delegate = inserter = EntityConfiguration.InsertionFactory.GetOrCreateInserter(DbConfiguration);
            }

            var affectedRows = await inserter.Invoke(UnderlyingCommand.Connection, new List<TEntity> { entity });

            if (DisposeCommand)
                this.Dispose();

            return affectedRows;
        }

        Task<IDeleteCommand<TEntity>> IDeleteCommand<TEntity>.PrepareAsync(CancellationToken cancellationToken)
        {
            return UnderlyingCommand.PrepareAsync(cancellationToken).ContinueWith(_ => (IDeleteCommand<TEntity>)this);
        }

        IDeleteCommand<TEntity> IDeleteCommand<TEntity>.Unprepare()
        {
            UnderlyingCommand.Unprepare();

            return this;
        }

        async Task<int> IDeleteCommand<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            EnsureValidConnection();

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
                this.Dispose();

            return affectedRows;
        }

        async Task<int> IDeleteCommand<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            EnsureValidConnection();

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
                this.Dispose();

            return affectedRows;
        }

        Task<IUpdateCommand<TEntity>> IUpdateCommand<TEntity>.PrepareAsync(CancellationToken cancellationToken)
        {
            return UnderlyingCommand.PrepareAsync(cancellationToken).ContinueWith(_ => (IUpdateCommand<TEntity>)this);
        }

        IUpdateCommand<TEntity> IUpdateCommand<TEntity>.Unprepare()
        {
            UnderlyingCommand.Unprepare();

            return this;

        }

        private void EnsureValidConnection()
        {
            if (UnderlyingCommand.Connection is null || UnderlyingCommand.Connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException($"The connection state is invalid. Expected: 'Open'. Actual: '{UnderlyingCommand.Connection?.State.ToString() ?? "Connection is null"}'");
            }
        }

        public void Dispose()
        {
            if (UnderlyingCommand.IsPrepared)
            {
                UnderlyingCommand.Unprepare();
            }

            UnderlyingCommand.Dispose();
        }
    }

    public interface IVenflowCommand<TEntity> : IDisposable where TEntity : class
    {

    }
}
