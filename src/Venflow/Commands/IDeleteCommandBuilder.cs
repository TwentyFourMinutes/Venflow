using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the deletion.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be deleted.</typeparam>
    public interface IDeleteCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IDeleteCommand<TEntity>, IDeleteCommandBuilder<TEntity>> where TEntity : class, new()
    {
        /// <summary>
        /// Asynchronously deletes a single entity.
        /// </summary>
        /// <param name="entity">The entity instance which should be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        ValueTask<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deletes a set of entity.
        /// </summary>
        /// <param name="entities">The entity instances which should be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        ValueTask<int> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deletes a set of entity.
        /// </summary>
        /// <param name="entities">The entity instances which should be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        ValueTask<int> DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deletes a set of entity.
        /// </summary>
        /// <param name="entities">The entity instances which should be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        ValueTask<int> DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deletes a set of entity.
        /// </summary>
        /// <param name="entities">The entity instances which should be deleted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; -1 otherwise.</returns>
        ValueTask<int> DeleteAsync(TEntity[] entities, CancellationToken cancellationToken = default);
    }
}
