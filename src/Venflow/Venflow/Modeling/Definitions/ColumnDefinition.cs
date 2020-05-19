using Npgsql;
using System;

namespace Venflow.Modeling.Definitions
{
    internal class ColumnDefinition<TEntity> where TEntity : class
    {
        internal string Name { get; set; }

        internal Action<TEntity, NpgsqlDataReader>? ValueWriter { get; set; }

        internal ColumnDefinition(string name)
        {
            Name = name;
        }
    }
}
