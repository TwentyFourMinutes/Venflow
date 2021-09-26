namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command which performs queries and materialize the results to entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which represents the result of the query.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public interface IQueryCommand<TEntity, TReturn> : IVenflowCommand<TEntity> where TEntity : class, new() where TReturn : class, new()
    {
        /// <summary>
        /// Asynchronously prepares the current SQL command on the database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        Task<IQueryCommand<TEntity, TReturn>> PrepareAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously un-prepares the current SQL command on the database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        Task<IQueryCommand<TEntity, TReturn>> UnprepareAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously performs queries and materializes the result.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the materialized result of the query; <see langword="null"/> otherwise.</returns>
        Task<TReturn?> QueryAsync(CancellationToken cancellationToken = default);
    }
}
