using System;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling
{
    internal class PostgreEnumEntityColumn<TEntity> : EntityColumn<TEntity>, IPostgreEnumEntityColumn
        where TEntity : class, new()
    {
        internal PostgreEnumEntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, NpgsqlDbType dbType, ColumnInformation? information) : base(propertyInfo, columnName, valueRetriever, false, dbType, information)
        {

        }
    }

    internal interface IPostgreEnumEntityColumn
    {

    }
}
