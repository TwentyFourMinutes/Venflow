using System;

namespace Venflow.CodeFirst.Data
{

    public class MigrationDatabaseEntity
    {
        public string Name { get; set; }

        public string Checksum { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
