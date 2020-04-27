using Npgsql;
using System;

namespace Venflow
{
    internal class EntityColumn<TEntity> where TEntity : class
    {
        internal string ColumnName { get; }
        internal Func<TEntity, NpgsqlParameter> ParameterRetriever { get; }

        public EntityColumn(string columnName, Func<TEntity, NpgsqlParameter> parameterRetriever)
        {
            ColumnName = columnName;
            ParameterRetriever = parameterRetriever;
        }
    }
}
