using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command which performs updates of entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be updated.</typeparam>
    public interface IUpdateCommand<TEntity> : IVenflowCommand<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Asynchronously updates a single entity.
        /// </summary>
        /// <param name="entity">The change tracked entity instance which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously updates a set of entities.
        /// </summary>
        /// <param name="entity">The change tracked entity instance which should be updated.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}