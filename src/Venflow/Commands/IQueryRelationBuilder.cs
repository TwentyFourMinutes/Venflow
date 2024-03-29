﻿using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{
    /// <summary>
    /// Represents a query relation builder to configure the query.
    /// </summary>
    /// <typeparam name="TRelationEntity">The type of the entity which will be joined with.</typeparam>
    /// <typeparam name="TRootEntity">The root type of the entity.</typeparam>
    /// <typeparam name="TReturn">The return type of the query.</typeparam>
    public interface IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join. You are not forced to use this parameter! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to define it.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join. You are not forced to use this parameter! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to define it.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <param name="joinBehaviour">Configures the type of this join. You are not forced to use this parameter! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to define it.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized left joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized left joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized left joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenLeftWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized right joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized right joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized right joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenRightWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized full joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized full joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        /// <summary>
        /// Allows to configure materialized full joins for the current query.
        /// </summary>
        /// <typeparam name="TToEntity">The type of the joined entity.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the navigation property which should get joined on doing materialization.</param>
        /// <returns>An object that can be used to further configure the operation.</returns>
        /// <remarks>You are not forced to use these methods! It is just a more explicit way of defining your joins. Additionally Venflow will perform more validation in the background, if you choose to use these methods.</remarks>
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenFullWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }
}
