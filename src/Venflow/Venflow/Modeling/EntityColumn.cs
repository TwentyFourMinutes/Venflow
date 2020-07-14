using Npgsql;
using System;
using System.Reflection;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class
    {
        internal Action<TEntity, object> ValueWriter { get; }

        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, MethodInfo dbValueRetriever, Action<TEntity, object> valueWriter, Func<TEntity, string, NpgsqlParameter> valueRetriever) : base(propertyInfo, columnName, dbValueRetriever)
        {
            ValueWriter = valueWriter;
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal MethodInfo DbValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, MethodInfo dbValueRetriever)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            DbValueRetriever = dbValueRetriever;
        }
    }
}
