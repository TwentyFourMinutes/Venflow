using Npgsql;

namespace Venflow.Commands
{
    public interface IVenflowCommandBuilder<TEntity> : IQueryCommandBuilder<TEntity>, IInsertCommandBuilder<TEntity>, IDeleteCommandBuilder<TEntity>, IUpdateCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity> Query();
        IQueryCommandBuilder<TEntity> Query(string sql);
        IQueryCommandBuilder<TEntity> Query(string sql, params NpgsqlParameter[] parameters);

        IInsertCommandBuilder<TEntity> Insert();

        IDeleteCommandBuilder<TEntity> Delete();

        IUpdateCommandBuilder<TEntity> Update();
    }
}