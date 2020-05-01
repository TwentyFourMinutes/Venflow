using Npgsql;
using System;

namespace Venflow.Modeling
{

    internal class ColumnDefinition<TEntity> where TEntity : class
    {
        public string Name { get; set; }

        public Action<TEntity, NpgsqlDataReader>? ValueWriter { get; set; }

        public ColumnDefinition(string name)
        {
            Name = name;
        }
    }
}
