using System;
using System.Reflection;
using Npgsql;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isNullableReferenceType) : base(propertyInfo, columnName, isNullableReferenceType)
        {
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal bool IsNullableReferenceType { get; }

        protected EntityColumn(PropertyInfo propertyInfo, string columnName, bool isNullableReferenceType)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            IsNullableReferenceType = isNullableReferenceType;
        }
    }
}
