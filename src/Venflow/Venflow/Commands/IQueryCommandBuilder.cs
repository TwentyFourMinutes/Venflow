using Npgsql;

namespace Venflow.Commands
{
    public interface IQueryCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity> TrackChanges(bool trackChanges = true);

        IQueryCommand<TEntity> Single();
        IQueryCommand<TEntity> Single(string sql, params NpgsqlParameter[] parameters);

        IQueryCommand<TEntity> Batch();
        IQueryCommand<TEntity> Batch(ulong count);
        IQueryCommand<TEntity> Batch(string sql, params NpgsqlParameter[] parameters);
    }
}