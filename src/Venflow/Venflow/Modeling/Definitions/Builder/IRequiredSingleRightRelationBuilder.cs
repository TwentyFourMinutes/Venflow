using System;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IRequiredSingleRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithOne(Expression<Func<TRelation, TEntity>> navigationProperty);
    }
}
