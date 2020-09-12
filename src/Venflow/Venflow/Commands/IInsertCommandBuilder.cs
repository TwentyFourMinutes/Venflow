using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the insertion.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be inserted.</typeparam>
    public interface IInsertCommandBuilder<TEntity> : ISpecficVenflowCommandBuilder<IInsertCommand<TEntity>> where TEntity : class, new()
    {
        /// <summary>
        /// Defines if the insertion should populate identity columns.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> SetIdentityColumns();
        /// <summary>
        /// Defines if the insertion should populate all foreign properties.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> PopulateRelation();
        /// <summary>
        /// Defines if the insertion shouldn't populate identity columns.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> DoNotSetIdentityColumns();
        /// <summary>
        /// Defines if the insertion shouldn't populate all foreign properties.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IInsertCommandBuilder<TEntity> DoNotSetPopulateRelation();

        /// <summary>
        /// Asynchronously inserts a single entity.
        /// </summary>
        /// <param name="entity">The entity instance which should be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; 0 otherwise.</returns>
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously inserts a set of entities.
        /// </summary>
        /// <param name="entities">The entity instances which should be inserted.</param>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the number of rows affected if known; 0 otherwise.</returns>
        Task<int> InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken = default);
    }
}