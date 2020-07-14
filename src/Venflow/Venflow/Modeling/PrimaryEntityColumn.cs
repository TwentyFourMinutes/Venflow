using Npgsql;
using System;
using System.Reflection;

namespace Venflow.Modeling
{
    internal class PrimaryEntityColumn<TEntity> : EntityColumn<TEntity>, IPrimaryEntityColumn where TEntity : class
    {
        public bool IsServerSideGenerated { get; }

        internal PrimaryEntityColumn(PropertyInfo propertyInfo, string columnName, MethodInfo dbValueRetriever, Action<TEntity, object> valueWriter, Func<TEntity, string, NpgsqlParameter> valueRetriever, bool isServerSideGenerated) : base(propertyInfo, columnName, dbValueRetriever, valueWriter, valueRetriever)
        {
            IsServerSideGenerated = isServerSideGenerated;
        }
    }

    internal interface IPrimaryEntityColumn
    {
        bool IsServerSideGenerated { get; }
    }
}
