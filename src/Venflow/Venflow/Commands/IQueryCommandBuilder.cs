using System;
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
    public interface IQueryCommandBuilder<TEntity, TReturn> : ISpecficVenflowCommandBuilder<IQueryCommand<TEntity, TReturn>> where TEntity : class, new() where TReturn : class, new()
    {
        /// <summary>
        /// Determines whether or not to return change tracked entities from the query.
        /// </summary>
        /// <param name="trackChanges">Determines if change tracking should be applied.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryCommandBuilder<TEntity, TReturn> TrackChanges(bool trackChanges = true);

        /// <summary>
        /// Determines whether or not to log the query to the provided loggers.
        /// </summary>
        /// <param name="shouldLog">Determines if this query should be logged. This is helpful, if you configured the default logging behavior to be <see langword="true"/>.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You can configure the loggers in the <see cref="Database.Configure(DatabaseOptionsBuilder)"/> method with the <see cref="DatabaseOptionsBuilder.LogTo(Action{string}, bool)"/> methods.</remarks>
        IQueryCommandBuilder<TEntity, TReturn> Log(bool shouldLog = true);

        /// <summary>
        /// Logs the query to the provided <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The logger which is being used for this query.</param>
        /// <param name="includeSensitiveData">Determines whether or not to show populated parameters in this query.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>Be aware, that once you configure a logger on a query, the global configured loggers won't be executed for this query.</remarks>
        IQueryCommandBuilder<TEntity, TReturn> LogTo(Action<string> logger, bool includeSensitiveData);

        /// <summary>
        /// Logs the query to the provided <paramref name="loggers"/>.
        /// </summary>
        /// <param name="loggers">The loggers which are being used for this query.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>Be aware, that once you configure one or more loggers on a query, the global configured loggers won't be executed for this query.</remarks>
        IQueryCommandBuilder<TEntity, TReturn> LogTo(params (Action<string> logger, bool includeSensitiveData)[] loggers);

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