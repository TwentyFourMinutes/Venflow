using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{
    public interface IQueryCommandBuilder<TEntity, TReturn> : ISpecficVenflowCommandBuilder<IQueryCommand<TEntity, TReturn>> where TEntity : class, new() where TReturn : class, new()
    {
        IQueryCommandBuilder<TEntity, TReturn> TrackChanges(bool trackChanges = true);

        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
        JoinBuilder<TEntity, TToEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new();
    }
}