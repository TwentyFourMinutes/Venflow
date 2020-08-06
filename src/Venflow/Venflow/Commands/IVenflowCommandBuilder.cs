using Npgsql;
using System;
using System.Collections.Generic;

namespace Venflow.Commands
{
    public interface IVenflowCommandBuilder<TEntity> where TEntity : class
    {
        IPreCommandBuilder<TEntity, TEntity> QuerySingle();
        IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql);
        IPreCommandBuilder<TEntity, TEntity> QuerySingle(string sql, params NpgsqlParameter[] parameters);
        IPreCommandBuilder<TEntity, TEntity> QueryInterpolatedSingle(FormattableString sql);
        IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch();
        IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(ulong count);
        IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql);
        IPreCommandBuilder<TEntity, List<TEntity>> QueryBatch(string sql, params NpgsqlParameter[] parameters);
        IPreCommandBuilder<TEntity, List<TEntity>> QueryInterpolatedBatch(FormattableString sql);

        IInsertCommandBuilder<TEntity> Insert();

        IDeleteCommandBuilder<TEntity> Delete();

        IUpdateCommandBuilder<TEntity> Update();
    }
}