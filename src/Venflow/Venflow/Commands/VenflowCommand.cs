using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);

            if (!await reader.ReadAsync())
            {
                return default!;
            }

            var isChangeTracking = TrackingChanges && EntityConfiguration.ChangeTrackerFactory is { };

            var factory = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer(JoinBuilderValues, DbConfiguration, reader.GetColumnSchema());

            var entity = (await factory(reader)).FirstOrDefault(); // TODO: Refactor code to build more efficient materializer

            if (DisposeCommand)
                this.Dispose();

            return entity;
        }

        async Task<List<TEntity>> IQueryCommand<TEntity>.QueryBatchAsync(CancellationToken cancellationToken)
        {
            var isChangeTracking = TrackingChanges && EntityConfiguration.ChangeTrackerFactory is { };

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

            var factory = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer(JoinBuilderValues, DbConfiguration, reader.GetColumnSchema());

            var entities = await factory(reader);

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

        Task<IDeleteCommand<TEntity>> IDeleteCommand<TEntity>.PrepareAsync(CancellationToken cancellationToken)
        {
            return UnderlyingCommand.PrepareAsync(cancellationToken).ContinueWith(_ => (IDeleteCommand<TEntity>)this);
        }

        IDeleteCommand<TEntity> IDeleteCommand<TEntity>.Unprepare()
        {
            UnderlyingCommand.Unprepare();

            return this;
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
