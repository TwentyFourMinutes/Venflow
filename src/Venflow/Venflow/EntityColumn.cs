using Npgsql;
using System;

namespace Venflow
{
    internal class EntityColumn<TEntity> where TEntity : class
    {
        internal string ColumnName { get; }

        internal Action<TEntity, NpgsqlDataReader, int> ValueWriter { get; }

        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        public EntityColumn(string columnName, Action<TEntity, NpgsqlDataReader, int> valueWriter, Func<TEntity, string, NpgsqlParameter> valueRetriever)
        {
            ValueWriter = valueWriter;
            ColumnName = columnName;
            ValueRetriever = valueRetriever;
        }
    }
}
