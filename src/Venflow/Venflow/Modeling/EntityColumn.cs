using Npgsql;
using System;
using System.Reflection;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> where TEntity : class
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal MethodInfo DbValueRetriever { get; }

        internal Action<TEntity, object> ValueWriter { get; }

        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, MethodInfo dbValueRetriever, Action<TEntity, object> valueWriter, Func<TEntity, string, NpgsqlParameter> valueRetriever)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            DbValueRetriever = dbValueRetriever;
            ValueWriter = valueWriter;
            ValueRetriever = valueRetriever;
        }
    }
}
