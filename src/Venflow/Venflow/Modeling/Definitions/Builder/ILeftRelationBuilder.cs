using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    /// <summary>
    /// This interface hosts relation methods for the left side of a relation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    public interface ILeftRelationBuilder<TEntity> where TEntity : class
    {
        /// <summary>
        /// <para>
        ///     Configures a relationship where this entity type has a reference that points to a single instance of the other type in the relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method with no parameters will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// <para>
        ///     After calling this method, you should chain a call to <see cref="INotRequiredMultiRightRelationBuilder{TEntity, TRelation}.WithMany"/> or <see cref="INotRequiredSingleRightRelationBuilder{TEntity, TRelation}.WithOne"/> to fully configure the relationship. Calling just this method without the chained call will not produce a valid relationship.
        /// </para>
        /// </summary>
        /// <typeparam name="TRelation">The entity type that this relationship targets.</typeparam>
        /// <param name="navigationProperty"> A lambda expression representing the reference navigation property on this entity type that represents the relationship (post => post.Blog). If no property is specified, the relationship will be configured without a navigation property on this end.</param>
        /// <returns>An object that can be used to configure the relationship.</returns>
        INotRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>(Expression<Func<TEntity, TRelation>> navigationProperty) where TRelation : class;

        /// <summary>
        /// <para>
        ///     Configures a relationship where this entity type has a reference that points to a single instance of the other type in the relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// <para>
        ///     After calling this method, you should chain a call to <see cref="IMultiRightRelationBuilder{TEntity, TRelation}.WithMany(Expression{Func{TRelation, IList{TEntity}}})"/> or <see cref="IRequiredSingleRightRelationBuilder{TEntity, TRelation}.WithOne(Expression{Func{TRelation, TEntity}})"/> to fully configure the relationship. Calling just this method without the chained call will not produce a valid relationship.
        /// </para>
        /// </summary>
        /// <typeparam name="TRelation">The entity type that this relationship targets.</typeparam>
        /// <returns>An object that can be used to configure the relationship.</returns>
        IRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>() where TRelation : class;

        /// <summary>
        /// <para>
        ///     Configures a relationship where this entity type has a collection that contains instances of the other type in the relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method with no parameters will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// <para>
        ///     After calling this method, you should chain a call to <see cref="INotRequiredSingleRightRelationBuilder{TEntity, TRelation}.WithOne"/> to fully configure the relationship. Calling just this method without the chained call will not produce a valid relationship.
        /// </para>
        /// </summary>
        /// <typeparam name="TRelation">The entity type that this relationship targets.</typeparam>
        /// <returns>An object that can be used to configure the relationship.</returns>
        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>(Expression<Func<TEntity, IList<TRelation>>> navigationProperty) where TRelation : class;

        /// <summary>
        /// <para>
        ///     Configures a relationship where this entity type has a collection that contains instances of the other type in the relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// <para>
        ///     After calling this method, you should chain a call to <see cref="IRequiredSingleRightRelationBuilder{TEntity, TRelation}.WithOne(Expression{Func{TRelation, TEntity}})"/> to fully configure the relationship. Calling just this method without the chained call will not produce a valid relationship.
        /// </para>
        /// </summary>
        /// <typeparam name="TRelation">The entity type that this relationship targets.</typeparam>
        /// <returns>An object that can be used to configure the relationship.</returns>
        IRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>() where TRelation : class;
    }
}
