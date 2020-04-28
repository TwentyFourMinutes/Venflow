using Npgsql;
using System;

namespace Venflow
{
    internal class PrimaryEntityColumn<TEntity> : EntityColumn<TEntity> where TEntity : class
    {
        internal Action<TEntity, object> ValueWriter { get; }

        internal bool IsServerSideGenerated { get; }

        internal PrimaryEntityColumn(string columnName, Func<TEntity, string, NpgsqlParameter> parameterRetriever, Action<TEntity, object> valueWriter, bool isServerSideGenerated) : base(columnName, parameterRetriever)
        {
            ValueWriter = valueWriter;
            IsServerSideGenerated = isServerSideGenerated;
        }
    }
}
