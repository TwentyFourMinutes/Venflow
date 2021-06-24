using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Venflow
{
    internal class DatabaseTransaction : IDatabaseTransaction
    {
        internal bool IsDisposed { get; private set; }

        private readonly NpgsqlTransaction _npgsqlTransaction;

        internal DatabaseTransaction(NpgsqlTransaction transaction)
        {
            _npgsqlTransaction = transaction;
        }

        void IDatabaseTransaction.Commit()
            => _npgsqlTransaction.Commit();

        Task IDatabaseTransaction.CommitAsync(CancellationToken cancellationToken)
            => _npgsqlTransaction.CommitAsync(cancellationToken);

        void IDatabaseTransaction.Release(string name)
            => _npgsqlTransaction.Release(name);

        Task IDatabaseTransaction.ReleaseAsync(string name, CancellationToken cancellationToken)
            => _npgsqlTransaction.ReleaseAsync(name, cancellationToken);

        void IDatabaseTransaction.Rollback()
            => _npgsqlTransaction.Rollback();

        void IDatabaseTransaction.Rollback(string name)
            => _npgsqlTransaction.Rollback(name);

        Task IDatabaseTransaction.RollbackAsync(CancellationToken cancellationToken)
            => _npgsqlTransaction.RollbackAsync(cancellationToken);

        Task IDatabaseTransaction.RollbackAsync(string name, CancellationToken cancellationToken)
            => _npgsqlTransaction.RollbackAsync(name, cancellationToken);

        void IDatabaseTransaction.Save(string name)
            => _npgsqlTransaction.Save(name);

        Task IDatabaseTransaction.SaveAsync(string name, CancellationToken cancellationToken)
            => _npgsqlTransaction.SaveAsync(name, cancellationToken);

        NpgsqlTransaction IDatabaseTransaction.GetNpgsqlTransaction()
            => _npgsqlTransaction;

        void IDisposable.Dispose()
        {
            IsDisposed = true;

            _npgsqlTransaction.Dispose();
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            IsDisposed = true;

            return new ValueTask(_npgsqlTransaction.CommitAsync());
        }
    }

    public interface IDatabaseTransaction : IAsyncDisposable, IDisposable
    {
        void Commit();
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Rollback();
        Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Save(string name);
        Task SaveAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        void Rollback(string name);
        Task RollbackAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        void Release(string name);
        Task ReleaseAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        NpgsqlTransaction GetNpgsqlTransaction();
    }
}
