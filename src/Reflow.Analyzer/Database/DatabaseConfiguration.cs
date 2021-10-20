using System.Collections.Generic;

namespace Reflow.Analyzer.Database
{
    internal class DatabaseConfiguration
    {
        public string FullDatabaseName { get; }
        public List<DatabaseTable> Properties { get; }

        public DatabaseConfiguration(string fullDatabaseName)
        {
            FullDatabaseName = fullDatabaseName;

            Properties = new();
        }
    }

    internal class DatabaseTable
    {
        public string Name { get; }
        public string FullTypeName { get; }

        public DatabaseTable(string name, string fullTypeName)
        {
            Name = name;
            FullTypeName = fullTypeName;
        }
    }
}
