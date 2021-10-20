using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    /// <summary>
    /// Instances of this class are returned from methods inside the <see cref="Table{TEntity}"/> class when using the Fluid API and it is not designed to be directly constructed in your application code.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRelation">The entity type that this relationship targets.</typeparam>
    public interface IMultiRightRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        /// <summary>
        /// <para>
        ///     Configures this as a one-to-many relationship.
        /// </para>
        /// <para>
        ///     Note that calling this method with no parameters will explicitly configure this side of the relationship to use no navigation property, even if such a property exists on the entity type. If the navigation property is to be used, then it must be specified.
        /// </para>
        /// </summary>
        /// <param name="navigationProperty"> A lambda expression representing the collection navigation property on the other end of this relationship (blog => blog.Posts). If no property is specified, the relationship will be configured without a navigation property on the other end of the relationship.</param>
        /// <returns>An object that can be used to configure the relationship.</returns>
        IForeignKeyRelationBuilder<TEntity, TRelation> WithMany(Expression<Func<TRelation, IList<TEntity>>> navigationProperty);
    }
}
