using System;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling
{
    internal class PrimaryEntityColumn<TEntity> : EntityColumn<TEntity>, IPrimaryEntityColumn where TEntity : class, new()
    {
        public bool IsServerSideGenerated { get; }

        internal PrimaryEntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isServerSideGenerated, NpgsqlDbType? dbType, ColumnInformation? information) : base(propertyInfo, columnName, valueRetriever, false, dbType, information)
        {
            IsServerSideGenerated = isServerSideGenerated;
        }
    }

    internal interface IPrimaryEntityColumn
    {
        bool IsServerSideGenerated { get; }
    }
}
