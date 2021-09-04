using System;
using System.Reflection;
using Npgsql;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, ColumnOptions options) : base(propertyInfo, columnName, options)
        {
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal ColumnOptions Options { get; }

        protected EntityColumn(PropertyInfo propertyInfo, string columnName, ColumnOptions options)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            Options = options;
        }
    }
}
