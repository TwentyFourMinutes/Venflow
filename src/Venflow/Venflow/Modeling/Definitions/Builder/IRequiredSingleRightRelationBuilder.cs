using System;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    /// <summary>
    /// This interface hosts relation methods for the right side of a relation.
    /// </summary>
    public interface IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        /// <summary>
        /// <para>
        ///     Configures this as a one-to-one relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method with no parameters will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// </summary>
        /// <param name="navigationProperty">A lambda expression representing the reference navigation property on the other end of this relationship (blog => blog.BlogInfo). If no property is specified, the relationship will be configured without a navigation property on the other end of the relationship.</param>
        /// <returns>An object that can be used to configure the relationship.</returns>
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne(Expression<Func<TRelation, TEntity>> navigationProperty);
    }
}
