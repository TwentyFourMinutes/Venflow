using System;
using System.Reflection;
using Npgsql;

namespace Venflow.Modeling
{
    internal class PrimaryEntityColumn<TEntity> : EntityColumn<TEntity>, IPrimaryEntityColumn where TEntity : class, new()
    {
        public bool IsServerSideGenerated { get; }

        internal PrimaryEntityColumn(PropertyInfo propertyInfo, string columnName, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isServerSideGenerated) : base(propertyInfo, columnName, valueRetriever, false)
        {
            IsServerSideGenerated = isServerSideGenerated;
        }
    }

    internal interface IPrimaryEntityColumn
    {
        bool IsServerSideGenerated { get; }
    }
}
