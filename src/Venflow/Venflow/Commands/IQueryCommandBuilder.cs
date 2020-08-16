using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

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
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
    }
}