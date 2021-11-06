using Npgsql;
using NpgsqlTypes;
using Venflow.Enums;

namespace Venflow.Modeling
{
    internal class EntityColumn<TEntity> : EntityColumn where TEntity : class, new()
    {
        internal Func<TEntity, string, NpgsqlParameter> ValueRetriever { get; }

        internal EntityColumn(
            PropertyInfo propertyInfo, 
            string columnName, 
            string queryColumnName, 
            Func<TEntity, string, NpgsqlParameter> valueRetriever, 
            NpgsqlDbType? dbType, 
            ColumnOptions options
            ) 
            : base(propertyInfo, columnName, queryColumnName, dbType, options)
        {
            ValueRetriever = valueRetriever;
        }
    }

    internal abstract class EntityColumn
    {
        internal string ColumnName { get; }
        internal string NormalizedColumnName { get; }

        internal PropertyInfo PropertyInfo { get; }
        internal NpgsqlDbType? DbType { get; }
        internal ColumnOptions Options { get; }

        protected EntityColumn(
            PropertyInfo propertyInfo, 
            string columnName,
            string normalizedColumnName,
            NpgsqlDbType? dbType, 
            ColumnOptions options
            )
        {
            PropertyInfo = propertyInfo;
            ColumnName = columnName;
            NormalizedColumnName = normalizedColumnName;
            DbType = dbType;
            Options = options;
        }
    }
}
