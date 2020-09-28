using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Commands
{

    public interface IInsertRelationBuilder<TRelationEntity, TRootEntity> : IBaseInsertRelationBuilder<TRelationEntity, TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector)
    where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> AndWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }
}
