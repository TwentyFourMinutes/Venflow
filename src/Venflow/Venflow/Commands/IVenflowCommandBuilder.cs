using Npgsql;
using System.Collections.Generic;

namespace Venflow.Commands
{
    public interface IVenflowCommandBuilder<TEntity> where TEntity : class
    {
        IQueryCommandBuilder<TEntity, TEntity> QuerySingle();
        IQueryCommandBuilder<TEntity, TEntity> QuerySingle(string sql);
        IQueryCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters);
        IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch();
        IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count);
        IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql);
        IQueryCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters);

        IInsertCommandBuilder<TEntity> Insert();

        IDeleteCommandBuilder<TEntity> Delete();

        IUpdateCommandBuilder<TEntity> Update();
    }
}