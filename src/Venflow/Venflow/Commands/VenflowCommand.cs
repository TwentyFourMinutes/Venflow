using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommand<TEntity> : IQueryCommand<TEntity>, IInsertCommand<TEntity>, IDeleteCommand<TEntity>, IUpdateCommand<TEntity> where TEntity : class
    {
        internal NpgsqlCommand UnderlyingCommand { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; set; }
        internal QueryMaterializer<TEntity>? QueryMaterializer { get; set; }
        internal bool IsSingle { get; set; }
        internal bool TrackingChanges { get; set; }
        internal bool GetComputedColumns { get; set; }
        internal bool DisposeCommand { get; set; }

        internal VenflowCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity)
        {
            UnderlyingCommand = underlyingCommand;
            EntityConfiguration = entity;
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
