using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Venflow.Commands
{
    public interface IBaseInsertRelationBuilder<TRelationEntity, TRootEntity> : IInsertCommandBuilder<TRootEntity>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
    {
        IBaseInsertRelationBuilder<TRootEntity, TRootEntity> InsertWithAll();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector)
            where TToEntity : class, new();

        IInsertRelationBuilder<TToEntity, TRootEntity> InsertWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector)
            where TToEntity : class, new();
    }
}
