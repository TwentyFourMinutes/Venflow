using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IMultiRightRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
        IForeignKeyRelationBuilder<TEntity, TRelation> WithMany(Expression<Func<TRelation, IList<TEntity>>> navigationProperty);
    }
}
