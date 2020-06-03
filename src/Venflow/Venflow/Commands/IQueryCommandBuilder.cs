using Npgsql;
using Venflow.Enums;

namespace Venflow.Commands
{
    public interface IQueryCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity> TrackChanges(bool trackChanges = true);
        IQueryCommandBuilder<TEntity> JoinWith<TEntity2>(JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TEntity2 : class;

        IQueryCommand<TEntity> Single();
        IQueryCommand<TEntity> Single(string sql, params NpgsqlParameter[] parameters);

        IQueryCommand<TEntity> Batch();
        IQueryCommand<TEntity> Batch(ulong count);
        IQueryCommand<TEntity> Batch(string sql, params NpgsqlParameter[] parameters);
    }
}