using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{
    public interface IQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> ThenWith<TToEntity>(Expression<Func<TRelationEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();
    }
}
