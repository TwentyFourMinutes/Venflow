using System.Collections.Generic;

namespace Venflow.CodeFirst
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
