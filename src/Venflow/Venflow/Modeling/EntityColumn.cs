using System;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isNullableReferenceType, uint? precision, uint? scale, NpgsqlDbType dbType) : base(propertyInfo, columnName, isNullableReferenceType, precision, scale, dbType)
        {
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }

        internal bool IsNullableReferenceType { get; }

        internal bool IsNullable => IsNullableReferenceType || Nullable.GetUnderlyingType(PropertyInfo.PropertyType) != null;

        internal uint? Precision { get; }
        internal uint? Scale { get; }

        internal NpgsqlDbType DbType { get; }

        protected EntityColumn(PropertyInfo propertyInfo, string columnName, bool isNullableReferenceType, uint? precision, uint? scale, NpgsqlDbType dbType)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            IsNullableReferenceType = isNullableReferenceType;
            Precision = precision;
            Scale = scale;
            DbType = dbType;
        }
    }
}
