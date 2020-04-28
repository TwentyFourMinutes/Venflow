using Npgsql;
using System;

namespace Venflow
{
    internal class PrimaryEntityColumn<TEntity> : EntityColumn<TEntity> where TEntity : class
    {
        internal Action<TEntity, object> PrimaryKeyWriter { get; }

        internal bool IsServerSideGenerated { get; }

        internal PrimaryEntityColumn(string columnName, Action<TEntity, NpgsqlDataReader, int> valueWriter, Func<TEntity, string, NpgsqlParameter> valueRetriever, Action<TEntity, object> primaryKeyWriter, bool isServerSideGenerated) : base(columnName, valueWriter, valueRetriever)
        {
            PrimaryKeyWriter = primaryKeyWriter;
            IsServerSideGenerated = isServerSideGenerated;
        }
    }
}
