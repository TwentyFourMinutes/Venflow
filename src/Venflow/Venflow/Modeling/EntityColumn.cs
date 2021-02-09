using System;
using System.Reflection;
using Npgsql;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isNullableReferenceType, bool isReadOnly) : base(propertyInfo, columnName, isNullableReferenceType, isReadOnly)
        {
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal bool IsNullableReferenceType { get; }

        internal bool IsReadOnly { get; }

        protected EntityColumn(PropertyInfo propertyInfo, string columnName, bool isNullableReferenceType, bool isReadOnly)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            IsNullableReferenceType = isNullableReferenceType;
            IsReadOnly = isReadOnly;
        }
    }
}
