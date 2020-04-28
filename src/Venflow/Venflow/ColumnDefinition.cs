using System;

namespace Venflow
{

    internal class ColumnDefinition<TEntity> where TEntity : class
    {
        public string Name { get; set; }

        public Action<TEntity, NpgsqlDataReaderType>? ValueWriter { get; set; }

        public ColumnDefinition(string name)
        {
            Name = name;
        }
    }
}
