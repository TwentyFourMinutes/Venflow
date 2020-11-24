using System.Collections.Generic;

namespace Venflow.Design
{
    internal class MigrationEntity
    {
        internal string Name { get; }
        internal Dictionary<string, MigrationColumn> Columns { get; }

        internal MigrationEntity(string name)
        {
            Name = name;
            Columns = new();
        }
    }
}
