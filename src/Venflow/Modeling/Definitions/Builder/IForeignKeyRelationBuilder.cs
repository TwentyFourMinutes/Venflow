using System;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    /// <summary>
    /// This interface hosts relation methods for the foreign key configurations.
    /// </summary>
    public interface IForeignKeyRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
    {
        /// <summary>
        /// Configures the property to be used as the foreign key for this relationship.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <param name="navigationProperty">A lambda expression representing the foreign key property (post => post.BlogId).</param>
        void UsingForeignKey<TKey>(Expression<Func<TEntity, TKey>> navigationProperty);

        /// <summary>
        /// Configures the property to be used as the foreign key for this relationship.
        /// </summary>
        /// <typeparam name="TKey">The type of the foreign key.</typeparam>
        /// <param name="navigationProperty">A lambda expression representing the foreign key property (post => post.BlogId).</param>
        void UsingForeignKey<TKey>(Expression<Func<TRelation, TKey>> navigationProperty);
    }
}
