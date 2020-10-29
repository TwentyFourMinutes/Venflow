using NpgsqlTypes;

namespace Venflow.Modeling.Definitions
{
    internal class ColumnDefinition
    {
        internal string Name { get; set; }

        internal ColumnInformationDefiniton? Information { get; set; }
        internal NpgsqlDbType? DbType { get; set; }
        internal bool? IsNullable { get; set; }

        internal ColumnDefinition(string name)
        {
            Name = name;

            if (VenflowConfiguration.PopulateColumnInformation)
                Information = new ColumnInformationDefiniton();
        }
    }
}
