using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface ILeftRelationBuilder<TEntity> where TEntity : class
    {
        INotRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>(Expression<Func<TEntity, TRelation>> navigationProperty) where TRelation : class;
        IRequiredMultiRightRelationBuilder<TEntity, TRelation> HasOne<TRelation>() where TRelation : class;
        INotRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>(Expression<Func<TEntity, IList<TRelation>>> navigationProperty) where TRelation : class;
        IRequiredSingleRightRelationBuilder<TEntity, TRelation> HasMany<TRelation>() where TRelation : class;
    }
}
