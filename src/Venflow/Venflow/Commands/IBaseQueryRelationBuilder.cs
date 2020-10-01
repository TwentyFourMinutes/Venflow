using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a base query relation builder to configure the query.
    /// </summary>
    /// <typeparam name="TRelationEntity">The type of the entity which will be joined with.</typeparam>
    /// <typeparam name="TRootEntity">The root type of the entity.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public interface IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IPreCommandBuilder<TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();
    }
}
