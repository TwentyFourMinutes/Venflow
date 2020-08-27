using System;
using Npgsql;

namespace Venflow.Modeling.Definitions
{
    internal class ColumnDefinition<TEntity> where TEntity : class, new()
    {
        internal string Name { get; set; }

        internal Action<TEntity, NpgsqlDataReader>? ValueWriter { get; set; }

        internal ColumnDefinition(string name)
        {
            Name = name;
        }
    }
}
