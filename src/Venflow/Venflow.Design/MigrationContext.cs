using System.Collections.Generic;

namespace Venflow.Design
{
    internal class MigrationContext
    {
        internal Dictionary<string, MigrationEntity> Entities { get; }

        internal MigrationContext()
        {
            Entities = new();
        }
    }
}
