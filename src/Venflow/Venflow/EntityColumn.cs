using Npgsql;
using System;

namespace Venflow
{
    internal class EntityColumn<TEntity> where TEntity : class
    {
        internal string ColumnName { get; }
        internal Func<TEntity, string, NpgsqlParameter> ParameterRetriever { get; }

        public EntityColumn(string columnName, Func<TEntity, string, NpgsqlParameter> parameterRetriever)
        {
            ColumnName = columnName;
            ParameterRetriever = parameterRetriever;
        }
    }
}
