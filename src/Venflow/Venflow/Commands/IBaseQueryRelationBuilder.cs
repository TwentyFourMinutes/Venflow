using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{

    public interface IBaseQueryRelationBuilder<TRelationEntity, TRootEntity, TReturn> : IPreCommandBuilder<TRootEntity, TReturn>
        where TRelationEntity : class, new()
        where TRootEntity : class, new()
        where TReturn : class, new()
    {
        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();

        IQueryRelationBuilder<TToEntity, TRootEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TRootEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin)
            where TToEntity : class, new();
    }
}
