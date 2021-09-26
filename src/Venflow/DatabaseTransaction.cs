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

            return _npgsqlTransaction.DisposeAsync();
        }
    }

    /// <summary>
    /// Represents a transaction to be made with a database.
    /// </summary>
    public interface IDatabaseTransaction : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a transaction save point.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        /// <remarks>This method does not cause a database roundtrip to be made. The savepoint creation statement will instead be sent along with the next command.</remarks>
        void Save(string name);

        /// <summary>
        /// Creates a transaction save point.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
        /// <remarks>This method does not cause a database roundtrip to be made, and will therefore always complete synchronously. The savepoint creation statement will instead be sent along with the next command.</remarks>
        Task SaveAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        void Rollback(string name);

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
        Task RollbackAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Releases a transaction from a pending savepoint state.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        void Release(string name);

        /// <summary>
        /// Releases a transaction from a pending savepoint state.
        /// </summary>
        /// <param name="name">The name of the savepoint.</param>
        /// <param name="cancellationToken">An optional token to cancel the asynchronous operation.</param>
        Task ReleaseAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Will return the underlying <see cref="NpgsqlTransaction"/>.
        /// </summary>
        /// <returns>The underlying <see cref="NpgsqlTransaction"/></returns>
        /// <remarks>
        /// Please do note, that if you call any of the Dispose methods on the <see cref="NpgsqlTransaction"/> instead of the <see cref="IDatabaseTransaction"/> ones, Venflow will never know about it being disposed. Therefore, always call one of the Dispose methods on the <see cref="IDatabaseTransaction"/> itself.
        /// </remarks>
        NpgsqlTransaction GetNpgsqlTransaction();
    }
}
