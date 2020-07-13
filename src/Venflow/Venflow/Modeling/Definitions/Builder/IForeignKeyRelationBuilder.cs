using System;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IForeignKeyRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        void UsingForeignKey<TKey>(Expression<Func<TEntity, TKey>> navigationProperty);

        void UsingForeignKey<TKey>(Expression<Func<TRelation, TKey>> navigationProperty);
    }
}
