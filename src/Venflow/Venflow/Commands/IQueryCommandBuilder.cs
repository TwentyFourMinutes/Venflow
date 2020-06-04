using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Commands
{
    public interface IQueryCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity> TrackChanges(bool trackChanges = true);

        JoinBuilder<TEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class;
        JoinBuilder<TEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TEntity, IEnumerable<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class;

        IQueryCommand<TEntity> Single();
        IQueryCommand<TEntity> Single(string sql, params NpgsqlParameter[] parameters);

        IQueryCommand<TEntity> Batch();
        IQueryCommand<TEntity> Batch(ulong count);
        IQueryCommand<TEntity> Batch(string sql, params NpgsqlParameter[] parameters);
    }
}