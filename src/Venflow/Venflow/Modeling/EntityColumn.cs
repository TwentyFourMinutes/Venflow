using System;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isNullableReferenceType, NpgsqlDbType? dbType, ColumnInformation? columnInformation) : base(propertyInfo, columnName, isNullableReferenceType, dbType, columnInformation)
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

        internal NpgsqlDbType? DbType { get; }
        internal ColumnInformation? Information { get; }

        protected EntityColumn(PropertyInfo propertyInfo, string columnName, bool isNullableReferenceType, NpgsqlDbType? dbType, ColumnInformation? information)
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            IsNullableReferenceType = isNullableReferenceType;
            DbType = dbType;
            Information = information;
        }
    }
}
