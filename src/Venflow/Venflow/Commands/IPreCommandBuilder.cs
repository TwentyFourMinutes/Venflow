using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a command builder to configure the query.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity which will be queried.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public interface IPreCommandBuilder<TEntity, TReturn> : IQueryCommandBuilder<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        /// <summary>
        /// Defines if <b>&gt;&lt;</b> should be replaced by automatically generated joins in your SQL.
        /// </summary>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryCommandBuilder<TEntity, TReturn> AddFormatter();

        /// <summary>
        /// Asynchronously performs queries and materializes the result.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token, which is used to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation, with the materialized result of the query; <see langword="null"/> otherwise.</returns>
#if !NET48
        [return: MaybeNull]
#endif
        Task<TReturn> QueryAsync(CancellationToken cancellationToken = default);
    }
}