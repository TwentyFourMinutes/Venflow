using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the update.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be updated.</typeparam>
    public interface IUpdateCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IUpdateCommand<TEntity>> where TEntity : class, new()
    {
        /// <summary>
        /// Asynchronously updates a single entity.
        /// </summary>
        /// <param name="entity">The change tracked entity instance which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously updates a set of entities.
        /// </summary>
        /// <param name="entities">The change tracked entity instances which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously updates a set of entities.
        /// </summary>
        /// <param name="entities">The change tracked entity instances which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously updates a set of entities.
        /// </summary>
        /// <param name="entities">The change tracked entity instances which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask UpdateAsync(TEntity[] entities, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously updates a set of entities.
        /// </summary>
        /// <param name="entities">The change tracked entity instances which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken = default);
    }
}