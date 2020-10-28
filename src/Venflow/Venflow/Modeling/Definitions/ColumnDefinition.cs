using System;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling.Definitions
{
    internal class ColumnDefinition<TEntity> where TEntity : class, new()
    {
        internal string Name { get; set; }

        internal ColumnInformationDefiniton? Information { get; }
        internal NpgsqlDbType DbType { get; set; }

        internal Action<TEntity, NpgsqlDataReader>? ValueWriter { get; set; }

        internal ColumnDefinition(string name)
        {
            Name = name;

            if (VenflowConfiguration.PopulateColumnInformation)
                Information = new ColumnInformationDefiniton();
        }
    }
}
